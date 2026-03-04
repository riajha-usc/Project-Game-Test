using System.Collections;
using UnityEngine;

public class SpawnManagerController : MonoBehaviour
{
    public GameObject enemy;
    public Vector3[] spawns;
    public bool safeZone = false;
    public float spawnInterval = 5.0f;

    void Start()
    {
        StartCoroutine(SpawnEnemy());
    }

    IEnumerator SpawnEnemy()
    {
        while (GameManager.Instance == null ||
               GameManager.Instance.currentState != GameManager.GameState.Playing)
        {
            yield return null;
        }

        if (safeZone || enemy == null || spawns.Length == 0)
            yield break;

        int spawnCount = 0;

        while (spawnCount < spawns.Length)
        {
            Vector3 spawnPos = spawns[spawnCount];

            Instantiate(enemy, spawnPos, Quaternion.identity);

            spawnCount++;

            if (spawnCount < spawns.Length)
                yield return new WaitForSeconds(spawnInterval);
        }
    }
}