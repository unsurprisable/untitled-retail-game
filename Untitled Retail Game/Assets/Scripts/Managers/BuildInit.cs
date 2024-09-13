using UnityEngine;
using UnityEngine.SceneManagement;

public class BuildInit : MonoBehaviour
{
    // called on application start
    private void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = PlayerPrefs.GetInt(SettingsMenuUI.PLAYER_PREFS_FPS_LIMIT, 240);

        SceneManager.LoadScene(1);
    }
}
