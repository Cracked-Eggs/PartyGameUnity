using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    public playerMovement _pm; // Reference to the playerMovement script

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player is touching ground
        if (other.CompareTag("Ground")) // Ensure only ground tagged objects set grounded
        {
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player is leaving the ground
        if (other.CompareTag("Ground")) // Ensure only ground tagged objects unset grounded
        {
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Keep the grounded state true as long as we stay on the ground
        if (other.CompareTag("Ground")) // Ensure only ground tagged objects keep grounded
        {
        }
    }
}
