using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour 
{
public bool isLocalInstance;
    
[Header("Health")]
public int health = 100;

[Header("UI")] public Slider healthBar;

private bool died;
    
[PunRPC]
public void TakeDamage(int _damage)
{
    health -= _damage;

    if (isLocalInstance)
    {
        healthBar.value = health;
    }

    if (health <= 0)
    {
        // Respawn

        if (isLocalInstance)
        {
            if (!died)
            {
                died = true;
                    
                RoomManager.Instance.SpawnPlayer();
                
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
}

