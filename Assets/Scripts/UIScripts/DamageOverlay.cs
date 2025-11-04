using UnityEngine;
using UnityEngine.UI;

public class DamageOverlay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Image overlay; // full-screen red Image

    [Header("Flash Settings")]
    [SerializeField] float flashAlpha = 0.45f;
    [SerializeField] float flashDuration = 0.25f;

    [Header("Low Health (last bite)")]
    [SerializeField] float lowHealthAlpha = 0.30f;
    [SerializeField] float pulseAmplitude = 0.12f;
    [SerializeField] float pulseSpeed = 2.0f;

    [Header("Safety / Debug")]
    [SerializeField] int topmostSortingOrder = 5000;
    [SerializeField] bool debugHotkeys = true;

    Coroutine fadeRoutine;
    Coroutine pulseRoutine;
    bool lowHealthActive;

    void Reset() { overlay = GetComponent<Image>(); }

    void Awake()
    {
        //Create the overlay Image
        if (!overlay) overlay = GetComponent<Image>();
        if (!overlay) overlay = GetComponentInChildren<Image>(true);
        if (!overlay) overlay = CreateOverlayHierarchy();
        if (!overlay.sprite)
        {
            overlay.sprite = MakeWhiteSprite();
            overlay.type = Image.Type.Sliced; 
        }

        overlay.raycastTarget = false;
        overlay.transform.SetAsLastSibling();
        EnsureFullScreenRect(overlay.rectTransform);
        SetBaseColorIfWhite(overlay); // make it red if still pure white
        SetAlpha(0f);

        /*
        foreach (var g in overlay.GetComponentsInParent<CanvasGroup>(true))
            if (g && g.alpha <= 0.001f) Debug.LogWarning($"[DamageOverlay] Parent CanvasGroup alpha=0 on {g.name}");
        */
        }
    public void OnHit(int bites, int maxBites)
    {
        bool shouldLowHealth = (maxBites - bites) <= 1;

        if (shouldLowHealth && !lowHealthActive) ActivateLowHealth();
        else if (!shouldLowHealth && lowHealthActive) DeactivateLowHealth();

        FlashOnce();
    }

    public void ClearAll()
    {
        DeactivateLowHealth();
        StopFade();
        SetAlpha(0f);
    }

    public void ForceSolid(float alpha = 0.6f)
    {
        DeactivateLowHealth();
        StopFade();
        SetAlpha(alpha);
    }

    void FlashOnce()
    {
        float target = lowHealthActive ? lowHealthAlpha : 0f;
        float from = Mathf.Max(GetAlpha(), flashAlpha);
        StartFade(from, target, flashDuration);
    }

    void ActivateLowHealth()
    {
        lowHealthActive = true;
        if (GetAlpha() < lowHealthAlpha) SetAlpha(lowHealthAlpha);
        StartPulse();
    }

    void DeactivateLowHealth()
    {
        lowHealthActive = false;
        StopPulse();
    }

    void StartPulse()
    {
        StopPulse();
        pulseRoutine = StartCoroutine(PulseCo());
    }

    void StopPulse()
    {
        if (pulseRoutine != null)
        {
            StopCoroutine(pulseRoutine);
            pulseRoutine = null;
        }
    }

    System.Collections.IEnumerator PulseCo()
    {
        float t = 0f;
        while (true)
        {
            t += Time.unscaledDeltaTime * (Mathf.PI * 2f * pulseSpeed);
            float a = lowHealthAlpha + Mathf.Sin(t) * (pulseAmplitude * 0.5f); 
            SetAlpha(Mathf.Clamp01(a));
            yield return null;
        }
    }

    void StartFade(float from, float to, float duration)
    {
        StopFade();
        fadeRoutine = StartCoroutine(FadeCo(from, to, duration));
    }

    void StopFade()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }

    System.Collections.IEnumerator FadeCo(float from, float to, float duration)
    {
        float t = 0f;
        SetAlpha(from);
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);
            SetAlpha(Mathf.Lerp(from, to, k));
            yield return null;
        }
        SetAlpha(to);
        fadeRoutine = null;
    }

    float GetAlpha() => overlay ? overlay.color.a : 0f;

    void SetAlpha(float a)
    {
        if (!overlay) return;
        var c = overlay.color;
        c.a = a;
        overlay.color = c;
    }


    Image CreateOverlayHierarchy()
    {
        var canvasGO = new GameObject("DamageOverlay_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = canvasGO.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = topmostSortingOrder;

        var imgGO = new GameObject("DamageOverlay_Image", typeof(RectTransform), typeof(Image));
        imgGO.transform.SetParent(canvasGO.transform, false);

        var rt = imgGO.GetComponent<RectTransform>();
        EnsureFullScreenRect(rt);

        return imgGO.GetComponent<Image>();
    }

    void EnsureFullScreenRect(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
    }

    void SetBaseColorIfWhite(Image img)
    {
        var c = img.color;
        if (Mathf.Approximately(c.r, 1f) && Mathf.Approximately(c.g, 1f) && Mathf.Approximately(c.b, 1f))
            img.color = new Color(1f, 0f, 0f, 0f);
    }

    Sprite MakeWhiteSprite()
    {
        var tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100f);
    }

    /*
    void Update()
    {
        if (!debugHotkeys) return;
        if (Input.GetKeyDown(KeyCode.F1)) { Debug.Log("[DamageOverlay] DEBUG Flash"); FlashOnce(); }
        if (Input.GetKeyDown(KeyCode.F2)) { Debug.Log("[DamageOverlay] DEBUG Solid"); ForceSolid(0.6f); }
        if (Input.GetKeyDown(KeyCode.F3)) { Debug.Log("[DamageOverlay] DEBUG Clear"); ClearAll(); }
    }
    */
}
