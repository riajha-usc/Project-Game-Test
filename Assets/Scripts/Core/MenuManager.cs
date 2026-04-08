using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    static void LoadSceneWithEntryMode(string sceneName, bool fromMainMenu)
    {
        if (GameManager.Instance != null)
        {
            if (fromMainMenu)
                GameManager.Instance.PrepareEntryFromMainMenu();
            else
                GameManager.Instance.PrepareNextLevelFromProgression();
        }

        SceneManager.LoadScene(sceneName);
    }

    public static void LoadTutorialKey() => LoadSceneWithEntryMode("Tutorial-1", fromMainMenu: true);
    public static void LoadTrapsPrototype() => LoadSceneWithEntryMode("Traps-Prototype", fromMainMenu: true);
    public static void LoadLevel1(bool fromMainMenu = true) => LoadSceneWithEntryMode("Level1", fromMainMenu);
    public static void LoadLevel2(bool fromMainMenu = true) => LoadSceneWithEntryMode("Level2", fromMainMenu);
    public static void LoadLevel3(bool fromMainMenu = true) => LoadSceneWithEntryMode("Level3", fromMainMenu);

    public static void LoadMainMenu() => SceneManager.LoadScene("MainMenu-Scene");

    public void OnKeyTutorialPressed() => LoadTutorialKey();
    public void OnTrapTutorialPressed() => LoadTrapsPrototype();
    public void OnLevel1Pressed() => LoadLevel1();
    public void OnLevel2Pressed() => LoadLevel2();
    public void OnLevel3Pressed() => LoadLevel3();
    public void StartMenuPressed() => LoadMainMenu();

    public static void LoadNextScene()
    {
        var current = SceneManager.GetActiveScene().name;

        switch (current)
        {
            case "Tutorial-1":
                LoadSceneWithEntryMode("Traps-Prototype", fromMainMenu: false);
                return;
            case "Traps-Prototype":
                LoadLevel1(fromMainMenu: false);
                return;
            case "Level1":
                LoadLevel2(fromMainMenu: false);
                return;
            case "Level2":
                LoadLevel3(fromMainMenu: false);
                return;
            default:
                LoadMainMenu();
                return;
        }
    }
}
