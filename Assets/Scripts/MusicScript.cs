using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public AudioSource source;

    void Start()
    {
        float vol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        source.volume = vol;
    }

    public void SetVolume(float v)
    {
        source.volume = v;
        PlayerPrefs.SetFloat("MusicVolume", v);
    }
}
