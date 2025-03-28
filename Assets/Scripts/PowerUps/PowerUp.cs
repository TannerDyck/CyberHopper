using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [Header("Power-up Settings")]
    public bool isInvincibilityPowerUp;
    public bool isSpeedBoostPowerUp;
    public float duration = 5f;
    public float speedMultiplier = 5f; // Only for Speed Boost

    [Header("Visual Effects")]
    public GameObject collectEffect;

    private PowerUpManager manager;
    private bool isCollected = false;

    // 🔹 Fix: Add this missing method
    public void SetManager(PowerUpManager manager)
    {
        this.manager = manager;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isCollected) return;

        if (other.CompareTag("Player"))
        {
            isCollected = true;
            ApplyPowerUpEffect(other.gameObject);

            if (collectEffect != null)
            {
                Instantiate(collectEffect, transform.position, Quaternion.identity);
            }

            PowerUpManager managerRef = this.manager ?? FindAnyObjectByType<PowerUpManager>();
            Destroy(gameObject);

            if (managerRef != null)
                managerRef.OnPowerUpCollected();
            else
                Debug.LogWarning("No PowerUpManager found!");
        }
    }

    public void ApplyPowerUpEffect(GameObject player)
    {
        Debug.Log("ApplyPowerUpEffect called for " + player.name);
        Frogger frogController = player.GetComponent<Frogger>();
        if (frogController == null)
        {
            Debug.LogError("Player object doesn't have a Frogger component!");
            return;
        }

        if (isInvincibilityPowerUp)
        {
            frogController.ActivateInvincibility(duration);
            Debug.Log("Invincibility activated for " + duration + " seconds!");
        }
        else if (isSpeedBoostPowerUp)
        {
            frogController.ActivateSpeedBoost(speedMultiplier, duration);
            Debug.Log("Speed Boost activated! Multiplier: " + speedMultiplier);
        }
        if (manager != null)
        {
            manager.OnPowerUpCollected();
        }
    }
}
