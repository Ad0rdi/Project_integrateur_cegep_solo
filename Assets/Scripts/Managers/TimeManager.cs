using Unity.Netcode;
using UnityEngine;

namespace Managers
{
    public static class TimeManager
    {
        public static bool isPaused { get; set; }


        public static void AskPause()
        {
            PauseServerRpc();
        }

        public static void AskResume()
        {
            ResumeServerRpc();
        }

        [ServerRpc]
        private static void PauseServerRpc()
        {
            PauseClientRpc();
        }

        [ClientRpc]
        private static void PauseClientRpc()
        {
            isPaused = true;
            Time.timeScale = 0;
        }

        [ServerRpc]
        private static void ResumeServerRpc()
        {
            ResumeClientRpc();
        }

        [ClientRpc]
        private static void ResumeClientRpc()
        {
            isPaused = false;
            Time.timeScale = 1;
        }
    }
}