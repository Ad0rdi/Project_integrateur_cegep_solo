/* Original auho name: Gilles Trahan
 * Creation date : 2025/04/07
 * Goal : Manage gun rotation and shooting
 *  Modification listing:
 *  2025/05/09:
 *      Author Name: Adam Turcotte
 *      Goal: Transform this into a controller to be use in player controller. Shoots its own bullets.
 */

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.Netcode;
using Reseau;


public class GunController : NetworkBehaviour
{
    [SerializeField] private GameObject bullet;
    [SerializeField] private Transform bulletSpawnPoint;
    [SerializeField] private GameObject playerCamera;
    private GameObject _screen;
    
    private void Start()
    {
        _screen = GameObject.FindWithTag("Screen");
    }

    // Update is called once per frame
    void Update()
    {
        RotationFusil();
        //Remplacer par clic de souris selon les conditions de coffres ou d'inventaires.
    }

    void RotationFusil()
    {
        Vector2 direction = PlayerToMouse.Get(_screen, playerCamera);

        transform.right = direction;

        Vector3 localScale = transform.localScale;
        if (Vector2.Angle(direction, Vector2.right) > 90f)
        {
            if (localScale.y > 0)
            {
                localScale.y *= -1;
            }
        }
        else
        {
            if (localScale.y < 0)
            {
                localScale.y *= -1;
            }
        }

        transform.localScale = localScale;
    }

    public void Shoot()
    {
        //Code de Gilles déplacé

        //Demande aux joueurs des autres instances de tirer
        AesSendShootRequest(bulletSpawnPoint.position, transform.rotation);

        //Le joueur tire sa propre balle en locale
        Instantiate(bullet, bulletSpawnPoint.position, transform.rotation);
    }

    #region RpcRequests

    /// <summary>
    ///  Fonction qui décrypte les données de tir en vérifiant qu'elles viennent de la bonne personne, avant de les propager aux autres clients
    /// </summary>
    /// <param name="encryptedPayload"></param>
    /// <param name="senderId"></param>
    /// <param name="rpcParams"></param>
    [ServerRpc]
    private void AesShootRequestServerRpc(byte[] encryptedPayload, ulong senderId, ServerRpcParams rpcParams = default)
    {
        //Vérifie que l'id du joueur correspond avec ce que le serveur croit
        if (rpcParams.Receive.SenderClientId != senderId) return;

        //Trouve la clé d'encryption du client
        if (!DictAesServeur.TryGetClientKey(senderId, out AesKeyData clientKey))
        {
            return;
        }

        byte[] decrypted = MathFunctions.DecryptAesData(encryptedPayload, clientKey.getKey(), clientKey.getIV());
        ShootData shootData = ShootData.FromByteArray(decrypted);

        //Vérifie que le l'id du joueur de la requête est le même que dans lea requête
        if (shootData.ID != senderId) return;


        //Crée ine liste avec tous les clients sauf celui qui a fait la demande, pour que seulement les autres la reçoivent
        List<ulong> clients = NetworkManager.ConnectedClientsIds.ToList();
        clients.Remove(senderId);
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clients
            }
        };

        //Demande au client de tirer
        ShootClientRpc(shootData, clientRpcParams);
    }

    /// <summary>
    ///  Permet de faire une demande de tir encryptée par Aes au serveur. Le <c>pos</c> et l'origine du tir, la <c>rot</c> est la rotation du vecteur direction
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    private void AesSendShootRequest(Vector3 pos, Quaternion rot)
    {
        //Récupère les données et le crypte pour la demande au serveur
        ulong id = ClientIdFromServer.GetId();
        ShootData data = new ShootData(id, pos, rot);
        byte[] plainBytes = data.ToByteArray();
        byte[] encryptedBytes = ClientKey.AesCrypter(plainBytes);

        //Envoie la demande de tir au serveur
        AesShootRequestServerRpc(encryptedBytes, id);
    }


    [ClientRpc]
    private void ShootClientRpc(ShootData shootData, ClientRpcParams rpcParams = default)
    {
        Instantiate(bullet, shootData.Position, shootData.GunRotation);
    }

    #endregion
}