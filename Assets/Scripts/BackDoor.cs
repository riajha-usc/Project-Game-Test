using UnityEngine;
using UnityEngine.SceneManagement;

public class BackDoor : MonoBehaviour
{
    public string sceneToLoad;
    public string spawnID;

    private void OnTriggerEnter(Collider other)
    {

        if (other.CompareTag("Player"))
        {
            GameManager.Instance.targetSpawnID = spawnID;
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
