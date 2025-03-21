using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    public float invincibilityDuration = 5f;
    public bool isInvincibilityPowerUp = true;

    [Header("Visual Effects")]
    public GameObject collectEffect; // Optional particle effect

    // Private reference to manager
    private PowerUpManager manager;
    private bool isCollected = false;

    // Public setter method for the manager
    public void SetManager(PowerUpManager manager)
    {
        this.manager = manager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent multiple collections
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            // Mark as collected to prevent multiple triggers
            isCollected = true;
            Debug.Log("Player collected power-up!");

            // Apply effects before destroying
            ApplyPowerUpEffect(other.gameObject);

            // Show collection effect if assigned
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            // Cache manager reference before destruction
            PowerUpManager managerRef = this.manager;

            // If no manager reference, try to find one
            if (managerRef == null)
            {
                managerRef = FindAnyObjectByType<PowerUpManager>();
            }

            // Destroy this object
            Destroy(gameObject);

            // After destroying, notify manager (if we have a reference)
            if (managerRef != null)
            {
                managerRef.OnPowerUpCollected();
            }
            else
            {
                Debug.LogWarning("No PowerUpManager found!");
            }
        }
    }

    private void ApplyPowerUpEffect(GameObject player)
    {
        if (isInvincibilityPowerUp)
        {
            // Get the Frogger component and activate invincibility
            Frogger frogController = player.GetComponent<Frogger>();
            if (frogController != null)
            {
                frogController.ActivateInvincibility(invincibilityDuration);
                Debug.Log("Invincibility activated for " + invincibilityDuration + " seconds!");
            }
            else
            {
                Debug.LogError("Player object doesn't have a Frogger component!");
            }
        }
    }
}