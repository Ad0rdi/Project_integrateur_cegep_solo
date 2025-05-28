using Managers;
using Unity.Netcode;
using UnityEngine;

namespace Networking
{
    public class NetworkGame: NetworkBehaviour
    {
        
            public void Start( )
            {
                StartServerRpc();
            }

            [ServerRpc]
            private void StartServerRpc( )
            {
                StartClientRpc();
            }

            [ClientRpc]
            private void StartClientRpc( )
            {
                FindAnyObjectByType<MenuManager>().ResumeTheGame();
            }
        
    }
}