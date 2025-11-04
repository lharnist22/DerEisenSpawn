using UnityEngine;
using UnityEngine.UI;

public class OptionsVolume : MonoBehaviour
{
    public Slider slider;

    void Start()
    {
        float vol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        slider.value = vol;
        slider.onValueChanged.AddListener(OnChanged);
    }

    void OnChanged(float v)
    {
        PlayerPrefs.SetFloat("MusicVolume", v);
        var music = FindFirstObjectByType<MusicPlayer>();
        if (music)
        {
            music.SetVolume(v);        
        }
    }
}
