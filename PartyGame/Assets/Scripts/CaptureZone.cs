using System.Collections;
using Photon.Pun;
using Photon.Pun.UtilityScripts;
using UnityEngine;

public class CaptureZone : MonoBehaviour
{
    bool isPlayerInZone = false;
    Coroutine scoreCoroutine;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
            scoreCoroutine = StartCoroutine(AddScoreEverySecond());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            // Stop the Coroutine when the player exits the zone
            if (scoreCoroutine != null)
            {
                StopCoroutine(scoreCoroutine);
                scoreCoroutine = null;
            }
        }
    }

    IEnumerator AddScoreEverySecond()
    {
        while (isPlayerInZone)
        {
            PhotonNetwork.LocalPlayer.AddScore(1);
            yield return new WaitForSeconds(1f); // Wait for 1 second
        }
    }
}