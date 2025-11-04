using UnityEngine;
using UnityEngine.UI; 

public class PointsManager : MonoBehaviour
{
    public static PointsManager Instance { get; private set; }

    [Header("UI (optional)")]
    [Tooltip("Drag a Text/TMP_Text that shows the current points (optional).")]
    public Text pointsText; 

    [Header("State")]
    public int Points { get; private set; } = 0;

    public event System.Action<int> OnPointsChanged;

    void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(transform.root.gameObject);
    }

    void Start()
    {
        UpdatePointsUI();
        OnPointsChanged?.Invoke(Points);
    }

    public void AddPoints(int amount)
    {
        if (amount == 0) return;
        SetPoints(Points + amount);
    }

    public bool TrySpend(int cost)
    {
        if (cost <= 0) return true; // no-op spend
        if (Points < cost) return false;
        SetPoints(Points - cost);
        return true;
    }

    public void ResetPoints(int start = 0)
    {
        SetPoints(start);
        Debug.Log($"[PointsManager] Points reset to {Points}");
    }

    public void UpdatePointsUI()
    {
        if (pointsText)
            pointsText.text = $"Points: {Points}";
    }

    // ----- Internals -----

    void SetPoints(int value)
    {
        Points = Mathf.Max(0, value);
       
        UpdatePointsUI();
        OnPointsChanged?.Invoke(Points);
    }
}
