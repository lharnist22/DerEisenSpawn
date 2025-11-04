using UnityEngine;
using UnityEngine.SceneManagement;

public class OptionsButton : MonoBehaviour
{
    public void Options()
    {
        SceneManager.LoadScene("OptionsScene");
    }
}
