using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

public class MainMenuManager : MonoBehaviour
{

    [SerializeField] private GameObject networkManager;
    [SerializeField] private GameObject gameLobby;

    [Space]

    [SerializeField] private TMP_InputField codeInputField;

    private void Awake()
    {
        if (NetworkManager.Singleton == null) {
            networkManager.SetActive(true);
        }
        if (GameLobby.Instance == null) {
            gameLobby.SetActive(true);
        }
    }
    
    private void Start()
    {
        DebugManager.instance.enableRuntimeUI = false;

        SteamManager.Instance.OnGameLobbyEntered += SteamManager_OnGameLobbyEntered;
    }

    private void SteamManager_OnGameLobbyEntered(object sender, EventArgs e)
    {
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

    public void JoinWithCodeFromClipboard()
    {
        TextEditor textEditor = new TextEditor();
        textEditor.Paste();
        SteamManager.Instance.JoinLobbyWithID(textEditor.text);
    }

    public void JoinWithCodeFromInputField()
    {
        SteamManager.Instance.JoinLobbyWithID(codeInputField.text);
        codeInputField.text = string.Empty;
    }
}
