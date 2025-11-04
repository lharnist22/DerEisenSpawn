using UnityEngine;

public class GameOverBootstrap : MonoBehaviour
{
    void Awake()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
