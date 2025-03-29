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

    private void Start()
    {
        if (manager == null)
        {
            manager = FindObjectOfType<PowerUpManager>();
        }
    }

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

            if (manager != null)
            {
                manager.OnPowerUpCollected();
            }
            else
            {
                // Fallback in case manager reference is lost
                PowerUpManager managerRef = FindObjectOfType<PowerUpManager>();
                if (managerRef != null)
                {
                    managerRef.OnPowerUpCollected();
                }
                else
                {
                    Debug.LogWarning("No PowerUpManager found!");
                }
            }

            Destroy(gameObject);
        }
    }

    private void ApplyPowerUpEffect(GameObject player)
    {
        if (player == null) return;

        Frogger frogController = player.GetComponent<Frogger>();
        if (frogController == null)
        {
            Debug.LogError("Player object doesn't have a Frogger component!");
            return;
        }

        if (isInvincibilityPowerUp)
        {
            frogController.ActivateInvincibility(duration);
            Debug.Log($"Invincibility activated for {duration} seconds!");
        }
        else if (isSpeedBoostPowerUp)
        {
            frogController.ActivateSpeedBoost(speedMultiplier, duration);
            Debug.Log($"Speed Boost activated! Multiplier: {speedMultiplier}");
        }
    }
}
