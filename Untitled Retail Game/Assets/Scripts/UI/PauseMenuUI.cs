using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuUI : Menu
{
    public static PauseMenuUI Instance { get; private set; }

    [SerializeField] private GameObject copyLobbyButton;

    private void Awake()
    {
        Instance = this;

        if (GameLobby.Instance != null && GameLobby.Instance.CurrentLobby != null) {
            copyLobbyButton.SetActive(true);
        }
    }

    public void ResumeGame()
    {
        Hide();
    }
    public void OpenSettings()
    {
        SettingsMenuUI.Instance.Show();
    }
    public void QuitToMenu()
    {
        if (GameLobby.Instance != null) {
            GameLobby.Instance.CurrentLobby?.Leave();
            GameLobby.Instance.CurrentLobby = null;
        } else {
            Debug.LogWarning("GameLobby is not initialized! Perhaps you didn't load the MainMenu scene yet?");
        }
        SceneManager.LoadScene("MainMenu");
    }
    public void QuitGame()
    {
        Application.Quit();
    }
    public void CopyLobbyCode()
    {
        if (GameLobby.Instance == null || GameLobby.Instance.CurrentLobby == null) {
            Debug.LogError("This button should not be enabled when a lobby connection is not established.");
            return;
        }

        TextEditor textEditor = new TextEditor();
        textEditor.text = GameLobby.Instance.CurrentLobby?.Id.ToString();
        textEditor.SelectAll();
        textEditor.Copy();
    }
}
