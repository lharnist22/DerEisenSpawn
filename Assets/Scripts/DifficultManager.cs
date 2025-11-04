using UnityEngine;
using UnityEngine.UI;

public class DifficultyManager : MonoBehaviour
{
    public Button hardModeButton;
    public Text buttonLabel; // optional – display ON/OFF text

    void Start()
    {
        bool isHard = PlayerPrefs.GetInt("HardMode", 0) == 1;
        UpdateButtonText(isHard);
        hardModeButton.onClick.AddListener(ToggleHardMode);
    }

    void ToggleHardMode()
    {
        bool isHard = PlayerPrefs.GetInt("HardMode", 0) == 1;
        bool newValue = !isHard;
        PlayerPrefs.SetInt("HardMode", newValue ? 1 : 0);
        PlayerPrefs.Save();
        Debug.Log("Hard Mode: " + (newValue ? "Enabled" : "Disabled"));
        UpdateButtonText(newValue);
    }

    void UpdateButtonText(bool isHard)
    {
        if (buttonLabel)
            buttonLabel.text = isHard ? "Hard Mode: ON" : "Hard Mode: OFF";
    }
}
