using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SettingsSlider : MonoBehaviour
{
    [SerializeField] private TMP_InputField valueInputField;
    [SerializeField] private Slider slider;
    public float Value {
        get => slider.value;
        set { slider.value = value; OnSliderValueChanged(value); }
    }

    private void Awake()
    {
        valueInputField.characterLimit = 3;
        valueInputField.lineLimit = 1;

        valueInputField.onValidateInput += ValidateInput;
    }

    private char ValidateInput(string text, int charIndex, char addedChar)
    {
        if (!int.TryParse(addedChar.ToString(), out int a)) {
            return '\0'; // not an integer
        }
        if (int.TryParse(text, out int result))
        {
            Debug.Log(result);
            if (result > slider.maxValue) {
                valueInputField.text = slider.maxValue.ToString();
                return '\0';
            } else if (result < slider.minValue) {
                valueInputField.text = slider.minValue.ToString();
                return '\0';
            }
        }
        return addedChar;
    }

    public void OnSliderValueChanged(float newValue)
    {
        valueInputField.text = newValue.ToString();
    }
}
