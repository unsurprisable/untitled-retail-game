using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : Menu
{
    public static PauseMenuUI Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public void ResumeGame()
    {
        Hide();
    }
    public void OpenSettings()
    {
        Debug.Log("settings are not a thing yet");
    }
    public void QuitToMenu()
    {
        GameLobby.Instance.CurrentLobby?.Leave();
        GameLobby.Instance.CurrentLobby = null;
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
}
