using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawnpoints : MonoBehaviour
{
    [SerializeField] Transform[] spawnpoints;  // Waypoints to move between
    public float moveSpeed = 5f;   // Speed of movement
    private int lastSpawnpoint = -1;  // Tracks last waypoint

    private void Start()
    {
        if (spawnpoints.Length > 0)
        {
            // Start the movement coroutine
            StartCoroutine(MoveSpawnpoints());
        }
    }

    private IEnumerator MoveSpawnpoints()
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
            yield return new WaitForSeconds(3f);
        }
    }
}
