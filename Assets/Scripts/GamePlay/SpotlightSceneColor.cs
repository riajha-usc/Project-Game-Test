using UnityEngine;
using UnityEngine.SceneManagement;

public class SpotlightSceneColor : MonoBehaviour
{
    [System.Serializable]
    public class SceneColorEntry
    {
        public string sceneName;
        public Color lightColor;
    }

    public SceneColorEntry[] sceneColors = new SceneColorEntry[]
    {
        new SceneColorEntry { sceneName = "StrangerThings-Lane1", lightColor = new Color(0.65f, 0.16f, 0.16f) },
        new SceneColorEntry { sceneName = "StrangerThings-Lane2", lightColor = new Color(0.65f, 0.16f, 0.16f) },
        new SceneColorEntry { sceneName = "StrangerThings-Lane3", lightColor = new Color(0.65f, 0.16f, 0.16f) }
    };

    public Color defaultColor = Color.white;

    private Light spotLight;

    private void Awake()
    {
        spotLight = GetComponent<Light>();
        if (spotLight == null)
            spotLight = GetComponentInChildren<Light>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        UpdateColorForCurrentScene();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateColorForCurrentScene();
    }

    private void UpdateColorForCurrentScene()
    {
        if (spotLight == null) return;

        string currentScene = SceneManager.GetActiveScene().name;

        foreach (var entry in sceneColors)
        {
            if (entry.sceneName == currentScene)
            {
                spotLight.color = entry.lightColor;
                return;
            }
        }

        spotLight.color = defaultColor;
    }
}
