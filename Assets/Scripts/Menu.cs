using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menu : MonoBehaviour
{
    [SerializeField] private string sceneName = "Multiplayer";

    public void OnHostButton() //Clique sur le bouton host
    {
        NetworkManager.Singleton.StartHost();
        NetworkManager.Singleton.SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }

    public void OnJoinButton() //Clique sur le bouton join
    {
        NetworkManager.Singleton.StartClient();
    }

    public void DropDown(int index)
    {
        sceneName = index switch //Change la map de départ selon le menu.
        {
            0 => "Multiplayer",
            1 => "Map_Generator",
            _ => sceneName
        };
    }
}