using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public int damage = 25;
    private PhotonView photonView;

    void Start()
    {
        photonView = GetComponent<PhotonView>(); // Get the PhotonView of the weapon owner
    }

    void OnCollisionEnter(Collision other)
    {
        PhotonView targetPhotonView = other.transform.gameObject.GetComponent<PhotonView>();

        // Check if the target has a PhotonView and it has a Health component
        if (targetPhotonView != null && other.transform.gameObject.GetComponent<Health>())
        {
            // Make sure the weapon does not damage the player who owns it
            if (!targetPhotonView.IsMine)
            {
                targetPhotonView.RPC("TakeDamage", RpcTarget.AllBuffered, damage);
            }
        }
    }
}