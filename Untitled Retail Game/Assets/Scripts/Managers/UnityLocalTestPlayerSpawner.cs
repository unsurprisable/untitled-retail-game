using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine;
using System;

public class UnityLocalTestPlayerSpawner : NetworkBehaviour
{
    public static UnityLocalTestPlayerSpawner Instance { get; private set; }

    [SerializeField] private Transform playerPrefab;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += NetworkManager_OnLoadEventCompleted;
        NetworkManager.Singleton.SceneManager.OnSynchronize += NetworkManager_OnSynchronize;
    }

    private void NetworkManager_OnSynchronize(ulong clientId)
    {
        if (IsHost)
        {
            SpawnPlayer(clientId);
        }
    }

    private void NetworkManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (IsHost && sceneName == "ExperimentalScene")
        {
            foreach (ulong clientId in clientsCompleted) {
                SpawnPlayer(clientId);
            }
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        Transform player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        
        // disabled accurate nametags because rn idk how to sync this to late-joining players

        // string nametag = clientId == 0 ? "(Host)" : "Client " + clientId;
        // player.GetComponent<PlayerNametagDisplay>().SetNametag(nametag);
    }
}
