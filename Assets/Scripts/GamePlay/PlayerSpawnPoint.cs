using UnityEngine;

public class PlayerSpawnPoint : MonoBehaviour
{
    public static PlayerSpawnPoint Instance;

    void Awake()
    {
        Instance = this;
    }
}