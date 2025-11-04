using TMPro;
using UnityEngine;

[DefaultExecutionOrder(+50)] // after PointsManager
public class PointsUI : MonoBehaviour
{
    [SerializeField] TMP_Text text;
    void Awake()
    {
        if (!text) text = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        if (PointsManager.Instance != null)
            PointsManager.Instance.OnPointsChanged -= HandleChanged;
    }

    void TrySubscribe()
    {
        // If manager not ready yet, retry next frame
        if (PointsManager.Instance == null)
        {
            StartCoroutine(RetryNextFrame());
            return;
        }

        PointsManager.Instance.OnPointsChanged += HandleChanged;
        HandleChanged(PointsManager.Instance.Points); // immediate init
    }

    System.Collections.IEnumerator RetryNextFrame()
    {
        yield return null;
        if (PointsManager.Instance != null)
        {
            PointsManager.Instance.OnPointsChanged += HandleChanged;
            HandleChanged(PointsManager.Instance.Points);
        }
    }

    void HandleChanged(int pts)
    {
        if (!text) return;
        text.text = $"Player: {pts:N0}";
    }

}
