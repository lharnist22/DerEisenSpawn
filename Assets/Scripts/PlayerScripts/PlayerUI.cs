using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public Text bitesText;
    public Image damageFlash;      // full-screen red image
    public Image crosshair;        

    [Header("Flash")]
    public Color flashColor = new Color(1f, 0f, 0f, 0.35f);
    public float flashTime = 0.15f;

    float flashT;                  // timer
    Color clear;                   // transparent red

    void Awake()
    {
        clear = new Color(flashColor.r, flashColor.g, flashColor.b, 0f);
        if (damageFlash) damageFlash.color = clear;
    }

    void Update()
    {
        // fade the flash back to transparent
        if (damageFlash && flashT > 0f)
        {
            flashT -= Time.deltaTime;
            float t = Mathf.Clamp01(flashT / flashTime);
            damageFlash.color = Color.Lerp(clear, flashColor, t * t);
        }
    }
}
