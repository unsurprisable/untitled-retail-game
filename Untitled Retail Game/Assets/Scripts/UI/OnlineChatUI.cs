using System;
using Steamworks;
using Steamworks.Data;
using TMPro;
using UnityEngine;

public class OnlineChatUI : MonoBehaviour
{
    public static OnlineChatUI Instance { get; private set; }

    [SerializeField] private Transform messageContainer;
    [SerializeField] private Transform messagePrefab;

    private bool isInOfflineMode;

    private void Awake()
    {
        if (GameLobby.Instance == null || GameLobby.Instance.currentLobby == null) {
            isInOfflineMode = true;
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SteamMatchmaking.OnLobbyMemberJoined += SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage += SteamMatchmaking_OnChatMessage;
        GameInput.Instance.OnJump += GameInput_OnJump;
    }

    private void GameInput_OnJump(object sender, EventArgs e)
    {
        GameLobby.Instance.currentLobby?.SendChatString(SteamClient.Name + " jumped!");
    }

    private void OnDisable()
    {
        if (isInOfflineMode) return;
        SteamMatchmaking.OnLobbyMemberJoined -= SteamMatchmaking_OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= SteamMatchmaking_OnLobbyMemberLeave;
        SteamMatchmaking.OnChatMessage -= SteamMatchmaking_OnChatMessage;
        GameInput.Instance.OnJump -= GameInput_OnJump;
    }

    private void SteamMatchmaking_OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        AddChatMessage(friend.Name + " joined the lobby");
    }

    private void SteamMatchmaking_OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        AddChatMessage(friend.Name + " left the lobby");
    }

    private void SteamMatchmaking_OnChatMessage(Lobby lobby, Friend friend, string message)
    {
        AddChatMessage(message);
    }

    public void AddChatMessage(string message)
    {
        Transform messageObj = Instantiate(messagePrefab, messageContainer);
        messageObj.GetComponent<TextMeshProUGUI>().text = message;
    }
}
