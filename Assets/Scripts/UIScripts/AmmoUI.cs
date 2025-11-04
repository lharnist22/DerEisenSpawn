using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    static AmmoUI instance;
    [SerializeField] TMP_Text text;

    void Awake()
    {
        instance = this;
        if (!text) text = GetComponent<TMP_Text>();
    }

    public static void NotifyChange(int inMag, int magSize, int reserve)
    {
        if (!instance || !instance.text) return;
        instance.text = instance.text ?? instance.GetComponent<TMP_Text>();
        instance.text.text = $"{inMag}/{magSize}   [{reserve}]";
    }
}
