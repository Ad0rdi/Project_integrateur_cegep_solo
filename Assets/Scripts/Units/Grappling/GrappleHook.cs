/* Original author name: Donavan Sirois
 * Creation date: 2025/04/22
 * Goal: Program the grapple hook object
 * Modification listing:
 * 2025/05/02:
 *      Author Name: Donavan Sirois
 *      Goal: Finished adding visual cue when not grappling
 * 2025/05/02:
 *      Author Name: Adam Turcotte
 *      Goal: Finished fishing velocity bug and multiplayer integration
 * 2025/05/06:
 *      Author Name: Donavan Sirois
 *      Goal: Added ceilingCheck to fix the wall clipping bug and modified the coroutine time for it to be constant
 * 2025/05/09:
 *      Author Name: Adam Turcotte
 *      Goal: Transform this into a controller to be use in player controller. Grapple is its own object.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Reseau;
using UnityEngine.Serialization;

public class GrappleHook : NetworkBehaviour
{
    [SerializeField] private LineRenderer _line;
    [SerializeField] LayerMask _ground;
    [SerializeField] private Transform _ceilingCheck;

    //Grapple parameters
    [SerializeField] float _maxDistance = 10f;
    [SerializeField] private float _minDistanceBeforeStopping = 0.6f;
    [SerializeField] float _grappleSpeed = 10f;
    [SerializeField] float _grappleShootSpeed = 20f;

    private bool _isBumping;
    private bool _isGrappling;
    [HideInInspector] public bool _retracting;

    private Vector2 _target;
    private Vector2 _2Dpos;

    //Network
    private bool _usingNetwork;

    //Player attributes
    [SerializeField] private Transform playerTransform;
    [SerializeField] private GameObject playerCamera;
    private Rigidbody2D _playerRigidbody2D;
    private GameObject _screen;

    //Cooldown
    [SerializeField] private float grappleCooldown = 1f;
    private float _cooldownEnd;
    private bool _canceled;

    public override void OnNetworkSpawn()
    {
        _usingNetwork = true;
        Awake();
    }

    private void Awake()
    {
        _playerRigidbody2D = GetComponentInParent<Rigidbody2D>();
        _screen = GameObject.FindWithTag("Screen");
        enabled = false;
    }

    private void Update()
    {
        if (!_retracting && !_isGrappling) enabled = false;
        if (!_retracting) return;

        if (_usingNetwork && !IsLocalPlayer)
        {
            //Pour que le grappling s'update automatiquement sur les autres clients
            UpdateGrapple();
            return;
        }

        _isBumping = Physics2D.OverlapCircle(_ceilingCheck.position, 0.1f, _ground);

        float dist = (_line.GetPosition(0) - _line.GetPosition(1)).magnitude;
        Vector2 grapplePos = Vector2.MoveTowards(playerTransform.position, _target, _grappleSpeed * Time.deltaTime);

        _playerRigidbody2D.MovePosition(grapplePos);

        _line.SetPosition(0, playerTransform.position);

        if (dist < _minDistanceBeforeStopping || _isBumping )
        {
            _retracting = false;
            _isGrappling = false;
            _line.enabled = false;
            _playerRigidbody2D.velocity = Vector2.zero;
            if (IsLocalPlayer) AesSendEndGrappleRequest();
        }
    }

    public void Shoot()
    {
        if (_isGrappling || !(Time.time > _cooldownEnd)) return;
        _canceled = false;
        StartGrapple(Vector2.zero);
        enabled = true;
    }

    public void Cancel()
    {
        if (!_isGrappling) return;
        _canceled = true;
        _cooldownEnd = Time.time + grappleCooldown;
        
        _retracting = false;
        _isGrappling = false;
        _line.enabled = false;
        _playerRigidbody2D.velocity = Vector2.zero;
        if (_usingNetwork && IsLocalPlayer) AesSendEndGrappleRequest();
        enabled = false;
    }

    private void UpdateGrapple()
    {
        Vector2 grapplePos = Vector2.Lerp(playerTransform.position, _target, _grappleSpeed * Time.deltaTime);
        _line.SetPosition(0, grapplePos);
    }

    private void StartGrapple(Vector2 canDirect)
    {
        Vector2 direction = PlayerToMouse.Get(_screen, playerCamera);

        if (_usingNetwork && !IsLocalPlayer)
            direction = canDirect;


        RaycastHit2D hit = Physics2D.Raycast(playerTransform.position, direction, _maxDistance, _ground);
        _2Dpos = new Vector2(playerTransform.position.x, playerTransform.position.y);


        if (hit.collider)
        {
            _target = hit.point;
        }
        else
        {
            _target = _2Dpos + direction.normalized * _maxDistance;
        }

        _isGrappling = true;
        _line.enabled = true;
        _line.positionCount = 2;

        StartCoroutine(Grapple(hit));
        if (IsLocalPlayer) AesSendGrappleRequest(direction);
    }

    IEnumerator Grapple(RaycastHit2D hit)
    {
        float _t = 0;
        float _time;
        if (hit.collider)
        {
            _time = hit.distance;
        }
        else
        {
            _time = 10;
        }

        _line.SetPosition(0, playerTransform.position);
        _line.SetPosition(1, playerTransform.position);

        Vector2 _newPos;

        for (; _t < _time; _t += _grappleShootSpeed * Time.deltaTime)
        {
            if (_canceled) yield break;
            _newPos = Vector2.Lerp(playerTransform.position, _target, _t / _time);
            _line.SetPosition(0, playerTransform.position);
            _line.SetPosition(1, _newPos);
            yield return null;
        }

        _line.SetPosition(1, _target);

        if (hit.collider)
        {
            _retracting = true;
        }
        else
        {
            _retracting = false;
            _isGrappling = false;
            _line.enabled = false;
        }
    }

    #region RpcRequests

    private void AesSendGrappleRequest(Vector2 direction)
    {
        ulong id = ClientIdFromServer.GetId();
        GrappleData data = new GrappleData(id, direction);
        byte[] plainBytes = data.ToByteArray();
        byte[] encrypteBytes = ClientKey.AesCrypter(plainBytes);

        AesGrappleRequestServerRpc(encrypteBytes, id);
    }

    private void AesSendEndGrappleRequest()
    {
        ulong id = ClientIdFromServer.GetId();
        byte[] data = BitConverter.GetBytes(id);
        byte[] encrypteBytes = ClientKey.AesCrypter(data);
        AesEndGrappleRequestServerRpc(encrypteBytes, id);
    }

    [ServerRpc]
    private void AesGrappleRequestServerRpc(byte[] encryptedBytes, ulong senderId, ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != senderId) return;
        if (!DictAesServeur.TryGetClientKey(senderId, out AesKeyData clientKey)) return;

        byte[] decryptedBytes = MathFunctions.DecryptAesData(encryptedBytes, clientKey.getKey(), clientKey.getIV());
        GrappleData grappleData = GrappleData.FromByteArray(decryptedBytes);

        if (grappleData.ID != senderId) return;

        List<ulong> clients = NetworkManager.ConnectedClientsIds.ToList();
        clients.Remove(senderId);
        ClientRpcParams clientRpcParams = new ClientRpcParams
            { Send = new ClientRpcSendParams { TargetClientIds = clients } };

        GrappleClientRpc(grappleData, clientRpcParams);
    }

    [ServerRpc]
    private void AesEndGrappleRequestServerRpc(byte[] encryptedBytes, ulong senderId,
        ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != senderId) return;
        if (!DictAesServeur.TryGetClientKey(senderId, out AesKeyData clientKey)) return;

        byte[] decryptedBytes = MathFunctions.DecryptAesData(encryptedBytes, clientKey.getKey(), clientKey.getIV());
        ulong id = BitConverter.ToUInt64(decryptedBytes, 0);
        if (senderId != id) return;


        List<ulong> clients = NetworkManager.ConnectedClientsIds.ToList();
        clients.Remove(senderId);
        ClientRpcParams clientRpcParams = new ClientRpcParams
            { Send = new ClientRpcSendParams { TargetClientIds = clients } };

        EndGrappleClientRpc(clientRpcParams);
    }

    [ClientRpc]
    private void GrappleClientRpc(GrappleData grappleData, ClientRpcParams rpcParams = default)
    {
        StartGrapple(grappleData.Direction);
        enabled = true;
    }

    [ClientRpc]
    private void EndGrappleClientRpc(ClientRpcParams rpcParams = default)
    {
        _retracting = false;
        _isGrappling = false;
        _line.enabled = false;
        enabled = false;
    }

    #endregion
}