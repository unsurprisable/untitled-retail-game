using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject gameLobby;
    [SerializeField] private GameObject steamManager;

    private void Awake()
    {
        if (NetworkManager.Singleton == null) {
            networkManager.SetActive(true);
        }
        if (GameLobby.Instance == null) {
            gameLobby.SetActive(true);
        }
        if (SteamManager.Instance == null) {
            steamManager.SetActive(true);
        }
    }
    
    private void Start()
    {
        SteamManager.Instance.OnGameLobbyEntered += SteamManager_OnGameLobbyEntered;
    }

    private void SteamManager_OnGameLobbyEntered(object sender, EventArgs e)
    {
        Debug.Log("entering game scene");
        SceneManager.LoadScene("ExperimentalScene");
    }

    public void HostPrivate() 
    {
        Debug.Log("entering private game scene; skipping lobby creation");
        SceneManager.LoadScene("ExperimentalScene");
    }

    public void HostFriendsOnly()
    {
        SteamManager.Instance.HostLobby(false);
    }

    public void HostPublic()
    {
        SteamManager.Instance.HostLobby(true);
    }
}
