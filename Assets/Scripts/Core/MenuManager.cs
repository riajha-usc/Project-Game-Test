using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void OnKeyTutorialPressed()
    {
        SceneManager.LoadScene("Tutorial-1");
    }

    public void OnTrapTutorialPressed()
    {
        SceneManager.LoadScene("Traps-Prototype");
    }

    public void OnLevel1Pressed()
    {
        SceneManager.LoadScene("Level1");
    }

    public void OnLevel2Pressed()
    {
        SceneManager.LoadScene("Level2");
    }

    public void OnLevel3Pressed()
    {
        SceneManager.LoadScene("Level3");
    }

    public void StartMenuPressed()
    {
        SceneManager.LoadScene("MainMenu-Scene");
    }
}