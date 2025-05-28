using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;
using Managers;

public class CustomSpawner : NetworkBehaviour
{
    public List<Transform> spawnPoints;
    private int nextSpawnIndex = 0;

    [SerializeField] public GameObject playerPrefab;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        // Get the next spawn point
        Transform spawn = GetNextSpawnPoint();

        // Instantiate the player object at the spawn point
        GameObject playerInstance = Instantiate(playerPrefab, spawn.position, spawn.rotation);

        // Spawn it with ownership
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

    }
    
    private Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogWarning("No spawn points assigned.");
            return transform;
        }

        Transform point = spawnPoints[nextSpawnIndex];
        nextSpawnIndex = (nextSpawnIndex + 1) % spawnPoints.Count;
        return point;
    }
    
    
}