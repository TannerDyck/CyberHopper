using UnityEngine;

public class PowerUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.CompareTag("Player"))  // Ensure player has the "Player" tag
        {
            print("Powerup hit!");
            Destroy(gameObject); // Remove the power-up after collection
        }
    }
}

