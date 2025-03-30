using System.Collections;
using UnityEngine;

public class Frogger : MonoBehaviour
{
    private Rigidbody2D frogger;
    private Vector3 spawnPosition;
    private bool isLeaping;

    private SpriteRenderer spriteRenderer;
    public Sprite idleSprite;
    public Sprite leapSprite;
    public Sprite deathSprite;

    // Invincibility variables
    private bool isInvincible = false;
    private float invincibilityDuration = 5f; // Duration in seconds
    public Color invincibilityColor = Color.white; // Changed to white for better visibility
    private Color originalColor;

    // Speed Boost variables
    private bool isSpeedBoosted = false;
    private float normalSpeed = 5f; // Normal speed
    private float currentSpeed = 5f;
    public Color speedBoostColor = new Color(0f, 1f, 1f, 1f); // Electric blue color
    private float multiplier = 1f;

    private float farthestRow;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        frogger = GetComponent<Rigidbody2D>();
        originalColor = spriteRenderer.color;

        // Set initial spawn position to bottom center
        spawnPosition = new Vector3(0f, -6f, 0f);
        transform.position = spawnPosition;
        farthestRow = spawnPosition.y;
        isLeaping = false;
    }

    private void Update()
    {
        if (isLeaping) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            Move(Vector3.up);
        }
        else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 180f);
            Move(Vector3.down);
        }
        else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            Move(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            transform.rotation = Quaternion.Euler(0f, 0f, -90f);
            Move(Vector3.right);
        }
    }

    private void Move(Vector3 direction)
    {
        Vector3 destination = transform.position + direction;

        Collider2D barrier = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Barrier"));
        Collider2D platform = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Platform"));
        Collider2D obstacle = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Obstacle"));
        Collider2D environment = Physics2D.OverlapBox(destination, Vector2.zero, 0f, LayerMask.GetMask("Environment"));

        // Check if destination has a Home component
        Collider2D[] colliders = Physics2D.OverlapBoxAll(destination, Vector2.zero, 0f);
        foreach (Collider2D col in colliders)
        {
            if (col.GetComponent<Home>() != null)
            {
                StartCoroutine(Leap(destination));
                return;
            }
        }

        // If we're not invincible, check for collisions and validate movement
        if (!isInvincible)
        {
            if (barrier != null || obstacle != null || (environment != null && platform == null))
            {
                Death();
                return;
            }
        }
        // When invincible, only check for barriers (to prevent going out of bounds)
        else if (barrier != null)
        {
            return;
        }

        if (platform != null)
        {
            transform.SetParent(platform.transform);
        }
        else
        {
            transform.SetParent(null);
        }

        if (destination.y > farthestRow)
        {
            farthestRow = destination.y;
            FindObjectOfType<GameManager>().AdvancedRow();
        }

        StartCoroutine(Leap(destination));
    }

    private IEnumerator Leap(Vector3 destination)
    {
        Debug.Log($"Leaping to: {destination}");
        isLeaping = true;
        Vector3 startPosition = transform.position;
        Vector3 roundedDestination = new Vector3(Mathf.Round(destination.x), Mathf.Round(destination.y), destination.z);

        // Play jump sound
        AudioManager.instance.PlaySFX("Frog Jump");

        float elapsed = 0f;
        float duration = 0.125f;

        spriteRenderer.sprite = leapSprite;

        while (elapsed < duration)
        {
            float t = elapsed / duration * multiplier; // reduce the leap time to simulate faster speed
            transform.position = Vector3.Lerp(startPosition, roundedDestination, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = roundedDestination;
        spriteRenderer.sprite = idleSprite;
        isLeaping = false;
    }

    public void Death()
    {
        Debug.Log("Frog died! Respawning...");
        StopAllCoroutines();
        isLeaping = false;

        // Play death sound
        AudioManager.instance.PlaySFX("Frog Death");

        transform.rotation = Quaternion.identity;
        spriteRenderer.sprite = deathSprite;
        enabled = false;

        if (frogger != null)
        {
            frogger.linearVelocity = Vector2.zero;
        }
        transform.SetParent(null);

        // Reset any active power-ups
        if (isInvincible) EndInvincibility();
        if (isSpeedBoosted) EndSpeedBoost();

        // Notify GameManager of death
        FindObjectOfType<GameManager>().FroggerDied();
    }

    public void Respawn()
    {
        if (this == null) return;  // Safety check

        Debug.Log($"Respawning at: {spawnPosition}");
        StopAllCoroutines();
        isLeaping = false;

        if (transform != null)
        {
            transform.rotation = Quaternion.identity;
            transform.position = spawnPosition;
            transform.SetParent(null);  // Ensure no parent transform is affecting position
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.sprite = idleSprite;
            spriteRenderer.color = originalColor;
        }

        if (frogger != null)
        {
            frogger.linearVelocity = Vector2.zero;
        }

        farthestRow = spawnPosition.y;
        enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;

        // Don't die if invincible
        if (isInvincible) return;

        // Check if it's a home first
        if (other.GetComponent<Home>() != null)
        {
            // Play home success sound
            AudioManager.instance.PlaySFX("Frog Home");
            return;
        }

        // Handle death when colliding with barriers, obstacles, or environment
        if (other.gameObject.layer == LayerMask.NameToLayer("Barrier") && transform.parent != null)
        {
            Death();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            Death();
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("Environment") && transform.parent == null)
        {
            Death();
        }
    }

    // Method to make the frog invincible
    public void ActivateInvincibility(float duration = 0f)
    {
        if (duration <= 0f) duration = invincibilityDuration;

        if (isInvincible)
        {
            CancelInvoke(nameof(EndInvincibility));
            StopCoroutine(nameof(FlashEffect));
        }

        isInvincible = true;
        StartCoroutine(FlashEffect(duration, true));
        Invoke(nameof(EndInvincibility), duration);
    }

    private void EndInvincibility()
    {
        isInvincible = false;
        StopCoroutine(nameof(FlashEffect));
        spriteRenderer.color = originalColor;
    }

    // Speed Boost Method
    public void ActivateSpeedBoost(float multiplier, float duration)
    {
        if (isSpeedBoosted)
        {
            CancelInvoke(nameof(EndSpeedBoost));
            StopCoroutine(nameof(FlashEffect));
        }

        this.multiplier = multiplier;
        isSpeedBoosted = true;

        StartCoroutine(FlashEffect(duration, false));
        Invoke(nameof(EndSpeedBoost), duration);
    }

    private void EndSpeedBoost()
    {
        isSpeedBoosted = false;
        StopCoroutine(nameof(FlashEffect));
        spriteRenderer.color = originalColor;
        currentSpeed = normalSpeed;
        multiplier = 1f;
    }

    private IEnumerator FlashEffect(float duration, bool isInvincibilityEffect)
    {
        float endTime = Time.time + duration;
        float baseFlashRate = 0.5f; // Start with 0.5s flash rate
        Color powerupColor = isInvincibilityEffect ? invincibilityColor : speedBoostColor;

        while (Time.time < endTime)
        {
            // Calculate remaining time
            float remainingTime = endTime - Time.time;

            // For invincibility, adjust flash rate based on remaining time
            float currentFlashRate = baseFlashRate;
            if (isInvincibilityEffect)
            {
                // Gradually decrease flash rate from 0.5s to 0.25s as time runs out
                currentFlashRate = Mathf.Lerp(0.25f, 0.5f, remainingTime / duration);
            }

            // Flash effect
            if (spriteRenderer.color.a > 0.5f)
            {
                spriteRenderer.color = new Color(powerupColor.r, powerupColor.g, powerupColor.b, 0.5f);
            }
            else
            {
                spriteRenderer.color = powerupColor;
            }

            yield return new WaitForSeconds(currentFlashRate);
        }

        spriteRenderer.color = originalColor;
    }
}
