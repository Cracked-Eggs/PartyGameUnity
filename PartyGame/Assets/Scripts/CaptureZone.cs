using UnityEngine;
using TMPro; // Import TextMesh Pro namespace

public class CaptureZone : MonoBehaviour
{
    public float timeInZone = 0f; // The time the player has stayed in the zone
    private bool isPlayerInZone = false;

    public TextMeshProUGUI timeText; // Reference to the TextMeshPro text

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
        }
    }

    private void Update()
    {
        // If the player is in the zone, accumulate time
        if (isPlayerInZone)
        {
            timeInZone += Time.deltaTime;
            // Update TextMeshPro text with rounded time
            timeText.text = "Time in Zone: " + Mathf.RoundToInt(timeInZone).ToString();
        }
    }
}
