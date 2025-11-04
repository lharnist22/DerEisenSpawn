using TMPro;
using UnityEngine;

public class RoundUI : MonoBehaviour
{
    [SerializeField] TMP_Text roundText;

    void Reset()
    {
        if (!roundText) roundText = GetComponent<TMP_Text>();
    }

    void OnEnable()
    {
        if (!roundText) roundText = GetComponent<TMP_Text>();
        TrySubscribe();
    }

    void OnDisable()
    {
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.OnRoundChanged -= HandleRoundChanged;
    }

    void TrySubscribe()
    {
        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.OnRoundChanged += HandleRoundChanged;
            // Initialize immediately to current round
            HandleRoundChanged(ZombieManager.Instance.currentRound);
        }
        else
        {
            // If manager isn’t ready yet, retry next frame
            StartCoroutine(SubscribeNextFrame());
        }
    }

    System.Collections.IEnumerator SubscribeNextFrame()
    {
        yield return null;
        if (ZombieManager.Instance != null)
        {
            ZombieManager.Instance.OnRoundChanged += HandleRoundChanged;
            HandleRoundChanged(ZombieManager.Instance.currentRound);
        }
    }

    void HandleRoundChanged(int round)
    {
        if (roundText)
            roundText.text = $"Round {round}";
    }
}
