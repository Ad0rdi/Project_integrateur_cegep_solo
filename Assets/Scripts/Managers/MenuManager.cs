using System.Linq;
using Networking;
using Reseau;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Managers
{
    public class MenuManager : MonoBehaviour
    {
        [SerializeField] public GameObject _EncryptionManager;
        [SerializeField] public GameObject _mainMenu;
        [SerializeField] public GameObject _gameUI;
        [SerializeField] public GameObject _pauseMenu;
        [SerializeField] public GameObject _deathMenu;
        [SerializeField] public GameObject _joinUI;
        [SerializeField] public GameObject _serverIp;
        [SerializeField] public GameObject _lobby_host;
        [SerializeField] public GameObject _lobby_client;

        [SerializeField] public GameObject playerPrefab;
        [SerializeField] public Transform spawnPoint;

        private bool _hostedOnce = false;

        private void Awake()
        {
            Time.timeScale = 0f;
            TimeManager.AskPause();
            _mainMenu.SetActive(true);
            _gameUI.SetActive(false);
            _pauseMenu.SetActive(false);
            _deathMenu.SetActive(false);
            _joinUI.SetActive(false);
            _serverIp.SetActive(false);
            _lobby_host.SetActive(false);
            _lobby_client.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown("escape"))
            {
                if (_pauseMenu.activeSelf)
                {
                    OnResumeButton();
                }
                else
                {
                    OnPauseButton();
                }
            }
        }

        public void ResumeTheGame()
        {
            _joinUI.SetActive(false);
            _mainMenu.SetActive(false);
            _serverIp.SetActive(true);
            _gameUI.SetActive(true);
            _lobby_host.SetActive(false);
            _lobby_client.SetActive(false);

            TimeManager.AskResume();
        }

        private void AddNetworkScripts(bool isHost)
        {
            FindAnyObjectByType<MapGenerator>().InitialiseParaNetworkList();

            Destroy(GameObject.FindGameObjectWithTag("Player")); //Removes solo player

            var rrs = _EncryptionManager.GetComponent<RsaReceiveServer>();
            if (isHost) rrs.enabled = true;
        }

        public void OnPlayButton()
        {
            CameraUtils.SetCameraOnPlayer();
            ResumeTheGame();
        }

        public void OnHostButton() //Clique sur le bouton host
        {
            Networking.NetworkFunction.SetOwnIP();
            _serverIp.SetActive(true);

            AddNetworkScripts(true);
            NetworkManager.Singleton.StartHost();

            if (!_hostedOnce)
                FindAnyObjectByType<MapGenerator>().SaveSettingsForNetwork();
            
            _mainMenu.SetActive(false);
            _lobby_host.SetActive(true);
            _hostedOnce = true;
            
        }

        public void OnStartHostButton()
        {
            var enemies = GetComponents<InverseKinematics>();
            foreach (var enemie in enemies)
            {
                enemie.FindPlayers();
            }
            FindFirstObjectByType<NetworkGame>().Start();
        }

        public void OnCancelLobbyHostButton()
        {
            NetworkManager.Singleton.Shutdown();
          Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            OnMainMenu();
        }

        public void OnJoinButton() //Clique sur le bouton join
        {
            _mainMenu.SetActive(false);
            var localIp = Networking.NetworkFunction.GetLocalIp();
            var trimmedLocalIp = localIp.Substring(0, localIp.LastIndexOf('.') + 1);

            var text = _joinUI.GetComponentsInChildren<Text>()
                .FirstOrDefault(t => t.name == "StartIp Text (Legacy)");

            if (text)
                text.text = trimmedLocalIp;

            _joinUI.SetActive(true);
        }

        public void OnConnectButton() //Clique sur le bouton connection
        {
            var text = _joinUI.GetComponentsInChildren<Text>().First(t => t.name == "EndIp Text (Legacy)");
            string endIp = text.text.Trim().Replace("\u200B", ""); //Enlever l'espace invisible;

            if (endIp.Length == 0) return;

            var localIp = Networking.NetworkFunction.GetLocalIp();
            var ip = localIp.Substring(0, localIp.LastIndexOf('.') + 1) + endIp;
            Networking.NetworkFunction.SetServerIp(ip);
            AddNetworkScripts(false);
            NetworkManager.Singleton.StartClient();

            _joinUI.SetActive(false);
            _lobby_client.SetActive(true);
        }

        public void OnCancelJoin()
        {
            _joinUI.SetActive(false);
            OnMainMenu();
        }

        public void OnCancelLobbyClientButton()
        {
            NetworkManager.Singleton.Shutdown();
            _lobby_client.SetActive(false);
            _joinUI.SetActive(true);
        }

        public void OnMainMenu()
        {
            _joinUI.SetActive(false);
            _lobby_host.SetActive(false);
            _lobby_client.SetActive(false);

            _mainMenu.SetActive(true);
        }

        public void OnQuitButton()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
            Application.Quit();
        }

        public void OnPauseButton()
        {
            TimeManager.AskPause();
            _pauseMenu.SetActive(true);
        }

        public void OnResumeButton()
        {
            TimeManager.AskResume();
            _pauseMenu.SetActive(false);
        }

        public void PlayAgain()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        public void GameOver()
        {
            _gameUI.SetActive(false);
            _deathMenu.SetActive(true);
            // Time.timeScale = 0f;
        }
    }
}