/* Original author name: Adam Turcotte
 * Creation date: 2025/05/01
 * Goal: Get and set server IP
 */
using System.Net;
using System.Net.Sockets;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace Networking
{
    public static class NetworkFunction
    {
        private static string ServerIp = "";
        public static string GetLocalIp()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }

            return null;
        }

        public static void SetServerIp(string ip)
        {
            ip = ip.Replace("\u200B", ""); // Supprimer U+200B
            ip = ip.Trim(); // Supprimer les espaces classiques
            
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData(ip,7777);
            ServerIp = ip;
        }

        public static string GetServerIp()
        {
            return ServerIp;
        }

        public static void SetOwnIP()
        {
            SetServerIp(GetLocalIp());
        }
    }
}