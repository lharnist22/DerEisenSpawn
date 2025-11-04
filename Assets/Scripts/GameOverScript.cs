using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameOverMenu : MonoBehaviour
{
    [Header("Scenes")]
    public string gameplaySceneName = "Gameplay";
    public string gameOverSceneName = "GameOver";

    void Awake()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 1f;

        EnsureEventSystem();
        EnsureGraphicRaycasterOnCanvases();
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene(gameOverSceneName, LoadSceneMode.Single);
    }

    public void PlayAgain()
    {
        SceneCleanup.KillZombiesAndGameSystems();
        SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    static void EnsureEventSystem()
    {
        var all = FindObjectsByType<EventSystem>(FindObjectsSortMode.None);

        if (all.Length == 0)
        {
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }
        else
        {
            for (int i = 1; i < all.Length; i++) Object.Destroy(all[i].gameObject);
        }
    }

    static void EnsureGraphicRaycasterOnCanvases()
    {
        foreach (var c in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
        {
            if (!c.TryGetComponent<GraphicRaycaster>(out _))
                c.gameObject.AddComponent<GraphicRaycaster>();
        }
    }
}
