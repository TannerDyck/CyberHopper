using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    public float invincibilityDuration = 5f;
    public bool isInvincibilityPowerUp = true;

    [Header("Visual Effects")]
    public GameObject collectEffect; // Optional particle effect

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Apply power-up effect
            ApplyPowerUpEffect(other.gameObject);

            // Show collection effect if assigned
            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            // Play sound effect if you have an audio source
            AudioSource audioSource = GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.Play();
                // Don't destroy immediately if we want to hear the sound
                Destroy(gameObject, audioSource.clip.length);
            }
            else
            {
                // Remove the power-up after collection
                Destroy(gameObject);
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
        }

        // can add more power-up types here in the future like speed boost, extra life, etc.
    }
}