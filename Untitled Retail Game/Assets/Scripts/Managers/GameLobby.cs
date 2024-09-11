using System.Collections;
using System.Collections.Generic;
using Steamworks.Data;
using UnityEngine;

public class GameLobby : MonoBehaviour
{
    public static GameLobby Instance { get; private set; }

    public Lobby? CurrentLobby;

    private void Awake()
    {
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
