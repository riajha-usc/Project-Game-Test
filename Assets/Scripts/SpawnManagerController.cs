using System;
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

    void Update()
    {

    }
    IEnumerator SpawnEnemy()
    {
        if (!safeZone)
        {
            for (int i = 0; i < spawns.Length; i++)
            {
                Vector3 spawnPos = new Vector3(
                    spawns[i].x,
                    spawns[i].y,
                    spawns[i].z);
                Instantiate(enemy, spawnPos, Quaternion.identity);
                yield return new WaitForSeconds(spawnInterval);
            }
        }
        else
        {
            Debug.Log("Enemy Prefabs not added!");
        }
    }
}
