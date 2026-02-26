using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverUIFail : MonoBehaviour
{
    public void RestartLevel()
    {
        Time.timeScale = 1f; // make sure time resumes
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
