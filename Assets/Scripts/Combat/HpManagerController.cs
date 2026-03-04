using UnityEngine;
using System.Collections;

public class HpManagerController : MonoBehaviour
{
    public GameObject hpBooster;
    public Vector3[] spawns;
    public float spawnInterval = 4.0f;
    void Start()
    {
        StartCoroutine(SpawnBoosters());
    }

    IEnumerator SpawnBoosters()
    {
       for (int i = 0; i < spawns.Length; i++)
       {
          Vector3 spawnPos = new Vector3(
               spawns[i].x,
               spawns[i].y,
               spawns[i].z);
           Instantiate(hpBooster, spawnPos, Quaternion.identity);
           yield return new WaitForSeconds(spawnInterval);
        }
    }
}
