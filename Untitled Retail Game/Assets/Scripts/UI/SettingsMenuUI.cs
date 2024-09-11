using System;
using UnityEngine;

public class SettingsMenuUI : SubMenu
{
    public static SettingsMenuUI Instance { get; private set; }

    public class OnSettingsValueChangedEventArgs : EventArgs {
        public float[] floatArgs;
        public int[] intArgs;
        public string[] stringArgs;
    }

    public event EventHandler<OnSettingsValueChangedEventArgs> OnSensitivityChanged;
    [SerializeField] private SettingsSlider sensX;
    private int SensX => (int)sensX.Value;
    [SerializeField] private SettingsSlider sensY;
    private int SensY => (int)sensY.Value;

    private const string PLAYER_PREFS_SENS_X = "MouseSensitivityX";
    private const string PLAYER_PREFS_SENS_Y = "MouseSensitivityY";

    private void InvokeOnSensitivityChanged() {
        OnSensitivityChanged?.Invoke(this, new OnSettingsValueChangedEventArgs {
            intArgs = new int[]{SensX, SensY}
        });
    }

    public event EventHandler<OnSettingsValueChangedEventArgs> OnFOVChanged;
    [SerializeField] private SettingsSlider fov;
    private int FOV => (int)fov.Value;

    private const string PLAYER_PREFS_FOV = "CameraFOV";

    private void InvokeOnFOVChanged() {
        OnFOVChanged?.Invoke(this, new OnSettingsValueChangedEventArgs {
            intArgs = new int[]{FOV}
        });
    }



    private void Awake()
    {
        Instance = this;

        SetMissingSettingsToDefault();
    }

    private void Start()
    {
        LoadSettings();
    }

    protected override void OnShow()
    {
        LoadSettings();
    }
    protected override void OnHide()
    {
        SaveSettings();
    }



    private void SetMissingSettingsToDefault()
    {
        SetMissingInt(PLAYER_PREFS_SENS_X, 10);
        SetMissingInt(PLAYER_PREFS_SENS_Y, 8);

        SetMissingInt(PLAYER_PREFS_FOV, 70);

        PlayerPrefs.Save();
    }
    private void SetMissingInt(string key, int value) {
        if (!PlayerPrefs.HasKey(key)) {
            PlayerPrefs.SetInt(key, value);
        }
    }
    private void SetMissingFloat(string key, float value) {
        if (!PlayerPrefs.HasKey(key)) {
            PlayerPrefs.SetFloat(key, value);
        }
    }
    private void SetMissingString(string key, string value) {
        if (!PlayerPrefs.HasKey(key)) {
            PlayerPrefs.SetString(key, value);
        }
    }


    private void LoadSettings()
    {
        Debug.Log("Retrieved PlayerPrefs settings; updating UI values accordingly.");

        sensX.Value = PlayerPrefs.GetInt(PLAYER_PREFS_SENS_X);
        sensY.Value = PlayerPrefs.GetInt(PLAYER_PREFS_SENS_Y);
        InvokeOnSensitivityChanged();

        fov.Value = PlayerPrefs.GetInt(PLAYER_PREFS_FOV);
        InvokeOnFOVChanged();
    }
    private void SaveSettings()
    {
        Debug.Log("Writing settings to PlayerPrefs...");

        PlayerPrefs.SetInt(PLAYER_PREFS_SENS_X, SensX);
        PlayerPrefs.SetInt(PLAYER_PREFS_SENS_Y, SensY);
        InvokeOnSensitivityChanged();

        PlayerPrefs.SetInt(PLAYER_PREFS_FOV, FOV);
        InvokeOnFOVChanged();

        PlayerPrefs.Save();
    }
}
