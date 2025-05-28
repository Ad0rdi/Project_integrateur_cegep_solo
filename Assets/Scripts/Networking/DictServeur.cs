/* Original author name: Adam Turcotte
 * Creation date: 2025/04/25
 * Goal: Class to hold all client encryption keys
 */

using System.Collections.Generic;

namespace Reseau
{
    public static class DictAesServeur
    {
        private static Dictionary<ulong, AesKeyData> ClientKeys = new();

        public static void RegisterClient(ulong clientId, AesKeyData key)
        {
            ClientKeys[clientId] = key;
        }

        public static bool TryGetClientKey(ulong clientId, out AesKeyData key)
        {
            return ClientKeys.TryGetValue(clientId, out key);
        }

        public static void UnregisterClient(ulong clientId)
        {
            ClientKeys.Remove(clientId);
        }

        public static void ClearAll()
        {
            ClientKeys.Clear();
        }
    }
}