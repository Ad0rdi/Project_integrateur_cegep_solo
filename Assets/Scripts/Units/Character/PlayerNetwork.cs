/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Handles player movement over server
 * Modification listing:
 * 2025/04/29:
 *      Author Name: Adam Turcotte
 *      Goal: correctly rotate gun
 */

using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Reseau;

namespace Units.Character
{
    /// <summary>
    /// An example network serializer with both server and owner authority.
    /// Love Tarodev
    /// </summary>
    public class PlayerTransform : NetworkBehaviour
    {
        [SerializeField] private Rigidbody2D playerBody;
        [SerializeField] private GameObject gun;
        [SerializeField] private GameObject player;
        private NetworkVariable<PlayerData> _playerState;

        //Pour que le joueur ne soit pas trop loin de la position réelle
        private float allowedDistance = 11.01f; // How close is "close enough"
        private Queue<Vector2> lastPositions = new();
        private const int maxTrackedPositions = 150;


        [SerializeField] private float cheapInterpolationTime = 0.1f;

        IEnumerator CallCameraUtils()
        {
            yield return new WaitForSeconds(0.2f);
            CameraUtils.SetCameraOnPlayer();
        }

        private void Awake()
        {
            _playerState = new NetworkVariable<PlayerData>();
            enabled = false; //Pour que la liste soit initialisé, sans qu'il commence à envoyer des donées inutiles
        }


        public override void OnNetworkSpawn()
        {
            enabled = true; //Pour qu'il commence à envoyer des donées au serveur
            if (IsLocalPlayer)
            {
                StartCoroutine(CallCameraUtils());
                player.name = "LocalPlayer";
                return;
            }

            player.name = "OnlinePlayer";
            //Détruit les control pour les autres joueurs
            GetComponent<PlayerMovement>().enabled = false;
            GetComponentInChildren<GrappleHook>().enabled = false;
            GetComponentInChildren<GunController>().enabled = false;
        }

        private void Update()
        {
            if (IsOwner) SendPlayerState();
            else
            {
                Consume();
                TrackPosition();
                CheckAndTeleport();
            }
        }

        #region RpcRequests

        private void SendPlayerState()
        {
            var id = ClientIdFromServer.GetId();
            var playerData = new PlayerData(id, playerBody.transform.position, playerBody.velocity,
                gun.transform.position, gun.transform.localScale);

            var payload = playerData.ToByteArray();
            var encryptedPayload = ClientKey.AesCrypter(payload);

            SendPlayerStateServerRpc(encryptedPayload, id);
        }

        [ServerRpc]
        private void SendPlayerStateServerRpc(byte[] encryptedPayload, ulong senderId,
            ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != senderId) return;
            if (!DictAesServeur.TryGetClientKey(senderId, out var key)) return;

            var payload = MathFunctions.DecryptAesData(encryptedPayload, key.getKey(), key.getIV());
            var playerData = new PlayerData().FromByteArray(payload);

            if (senderId != playerData.id) return;

            _playerState.Value = playerData;
        }

        #endregion

        #region Interpolate State

        private Vector3 _posVel;

        private void Consume()
        {
            playerBody.MovePosition(Vector3.SmoothDamp(playerBody.transform.position, _playerState.Value.position,
                ref _posVel, cheapInterpolationTime));
            gun.transform.right = _playerState.Value.GunPosition;
            var gunScale = gun.transform.localScale;
            gunScale.y = _playerState.Value.gunScale;
            gun.transform.localScale = gunScale;
        }

        private void TrackPosition()
        {
            // Ajouter la position actuel
            lastPositions.Enqueue(playerBody.transform.position);

            // Enlever s'il y en a trop
            if (lastPositions.Count > maxTrackedPositions)
            {
                lastPositions.Dequeue();
            }
        }

        private void CheckAndTeleport()
        {
            if (lastPositions.Count < maxTrackedPositions) return;
            bool allFar = true;
            foreach (Vector3 pos in lastPositions)
            {
                if (Vector3.Distance(pos, _playerState.Value.position) <= allowedDistance)
                {
                    allFar = false;
                    break;
                }
            }

            if (allFar)
            {
                playerBody.transform.position=Vector3.Lerp(playerBody.transform.position, _playerState.Value.position,0.8f);
                lastPositions.Clear(); // Reset tracking after teleport
            }
        }

        #endregion
    }
}