using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoints : MonoBehaviour
{
    [SerializeField] Transform[] spawnpoints;
    [SerializeField] int delay;
    int lastSpawnpoint = -1;

    void Start()
    {
        if (spawnpoints.Length > 0)
        {
            StartCoroutine(MoveSpawnpoints());
        }
    }

    IEnumerator MoveSpawnpoints()
    {
        while (true)
        {
            int random;
            do
            {
                random = Random.Range(0, spawnpoints.Length);
            }
            while (random == lastSpawnpoint);
            transform.position = spawnpoints[random].position;
            lastSpawnpoint = random;
            yield return new WaitForSeconds(delay);
        }
    }
}
