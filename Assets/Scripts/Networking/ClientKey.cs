/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Manages client AES key, encrypts en decrypts data using key
 */

using System.Security.Cryptography;
using UnityEngine;

namespace Reseau
{
    public class ClientKey : MonoBehaviour
    {
        //Variables des clés
        private static Aes _aes;
        private static RsaKeyData _serverPubKey;

        private void Awake()
        {
            //Crée la clé d'encryption symmétrique lors de la création du script
            _aes = Aes.Create();
            _aes.GenerateKey();
            _aes.GenerateIV();
        }

        /// <summary>
        ///  Crypte le <c>byte[]</c> avec la clé <c>Aes</c> du client  
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] AesCrypter(byte[] data)
        {
            return MathFunctions.CryptAesData(data, _aes.Key, _aes.IV);
        }


        public static void ClientConnected(byte[] rsaKeyData, ulong clientId)
        {
            ClientIdFromServer.SetId(clientId);
            _serverPubKey = RsaKeyData.FromByteArray(rsaKeyData);
            SendKeyToServer();
        }

        private static void SendKeyToServer()
        {
            AesKeyData keyData = new AesKeyData(_aes);
            byte[] keyblob = keyData.ToByteArray();

            var crpytedkey = MathFunctions.CryptRsaData(keyblob, _serverPubKey);

            var handler = FindFirstObjectByType<RsaReceiveServer>();
            if (handler != null)
            {
                handler.RegisterPrivateKeyServerRpc(crpytedkey, ClientIdFromServer.GetId());
            }
            else
            {
                Debug.LogError("[CLIENT] RsaReceiveServer non trouvé pour envoyer la clé.");
            }
        }
    }

    public class ClientIdFromServer : MonoBehaviour
    {
        private static ulong _id;

        public static void SetId(ulong id)
        {
            _id = id;
        }

        public static ulong GetId()
        {
            return _id;
        }
    }
}