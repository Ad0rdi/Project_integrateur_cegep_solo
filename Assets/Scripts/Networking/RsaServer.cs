/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Manage RSA key between client and server
 */
using Unity.Netcode;

namespace Reseau
{
    public class RsaReceiveServer : NetworkBehaviour
    {
        private RsaKeys _rsaKeys;


        private void Awake()
        {
            _rsaKeys = RsaKeys.Create();
        }

        private void OnEnable()
        {
            NetworkManager.Singleton.OnServerStarted += HandleServerStarted;
        }

        private void HandleServerStarted()
        {
            if (NetworkManager.IsServer || NetworkManager.IsHost)
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }

        private void OnDisable()
        {
            if (NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }

        private void OnClientConnected(ulong clientId)
        {
            byte[] keyBytes = new RsaKeyData(_rsaKeys.getPublicKey()).ToByteArray();
            SendOnClientConnectedServerRpc(keyBytes, clientId);
        }


        [ServerRpc]
        private void SendOnClientConnectedServerRpc(byte[] keyBytes, ulong clientId)
        {
            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = new[] { clientId }
                }
            };
            SendOnClientConnectedClientRpc(keyBytes, clientId, clientRpcParams);
        }


        [ClientRpc]
        private void SendOnClientConnectedClientRpc(byte[] keyBytes, ulong clientId, ClientRpcParams clientRpcParams)
        {
            ClientKey.ClientConnected(keyBytes, clientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void RegisterPrivateKeyServerRpc(byte[] encryptedKey, ulong senderId,
            ServerRpcParams rpcParams = default)
        {
            if (rpcParams.Receive.SenderClientId != senderId) return;


            var decrypted = MathFunctions.DecryptRsaData(encryptedKey, _rsaKeys.getPrivateKey());
            var aesKeyData = AesKeyData.FromByteArray(decrypted);

            DictAesServeur.RegisterClient(senderId, aesKeyData);
        }
    }
}