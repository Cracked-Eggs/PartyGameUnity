using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Health : MonoBehaviour
{
    public bool isLocalInstance;
    public int health = 100;

    [PunRPC]
    public void TakeDamage(int _damage)
    {
        health -= _damage;

        if (health <= 0)
        {
            if (isLocalInstance)
            {
                RoomManager.Instance.SpawnPlayer();
            }
            Destroy(gameObject);
        }
    }
}
