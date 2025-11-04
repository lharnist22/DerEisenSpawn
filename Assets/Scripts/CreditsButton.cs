using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsButton : MonoBehaviour
{
    public void Credits()
    {
        SceneManager.LoadScene("CreditsScene");
    }
}
