using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    public float Value {
        get => slider.value;
        set { slider.value = value; OnSliderValueChanged(value); }
    }

    [SerializeField] private TextMeshProUGUI valueText;

    public void OnSliderValueChanged(float newValue)
    {
        valueText.text = newValue.ToString();
    }
}
