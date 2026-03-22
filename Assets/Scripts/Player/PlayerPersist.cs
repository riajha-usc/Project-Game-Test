using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerPersist : MonoBehaviour
{
    private static PlayerPersist instance;

    void Awake()
    {
        string scene = SceneManager.GetActiveScene().name;
        if (!scene.StartsWith("Level"))
        {
            return;
        }

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
}