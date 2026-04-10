using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist instance;

    const string DontDestroySceneName = "DontDestroyOnLoad";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void RegisterSceneLoaded()
    {
        SceneManager.sceneLoaded -= OnSceneLoadedClearPersistedPlayer;
        SceneManager.sceneLoaded += OnSceneLoadedClearPersistedPlayer;
    }

    static void OnSceneLoadedClearPersistedPlayer(Scene scene, LoadSceneMode mode)
    {
        if (IsLevelSceneName(scene.name)) return;
        DestroyPersistedPlayerIfAny();
    }

    static bool IsLevelSceneName(string sceneName) =>
        !string.IsNullOrEmpty(sceneName) && sceneName.StartsWith("Level");

    static void DestroyPersistedPlayerIfAny()
    {
        instance = null;

        var players = Object.FindObjectsByType<PlayerMovement3D>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var pm in players)
        {
            if (pm == null) continue;
            GameObject go = pm.gameObject;
            if (go.scene.name != DontDestroySceneName) continue;
            go.SetActive(false);
            Object.Destroy(go);
        }
    }

    void Awake()
    {
        string scene = SceneManager.GetActiveScene().name;

        if (IsLevelSceneName(scene))
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            DestroyPersistedPlayerIfAny();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
