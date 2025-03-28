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
    public Color invincibilityColor = new Color(1f, 1f, 0.5f, 0.8f); // Yellow tint
    private Color originalColor;

    // Speed Boost variables
    private bool isSpeedBoosted = false;
    private float normalSpeed = 5f; // Normal speed
    private float currentSpeed = 5f;

    private float multiplier = 1f;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spawnPosition = transform.position;
        isLeaping = false;
        frogger = GetComponent<Rigidbody2D>();
        originalColor = spriteRenderer.color;
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

        // Check for collisions and validate movement
        if (barrier != null || obstacle != null || (environment != null && platform == null))
        {
            // Only call death if not invincible
            if (!isInvincible)
            {
                Death();
            }
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

        // If we're invincible, allow through barriers and obstacles
        StartCoroutine(Leap(destination));
    }

    private IEnumerator Leap(Vector3 destination)
    {
        Debug.Log($"Leaping to: {destination}");
        isLeaping = true;
        Vector3 startPosition = transform.position;
        Vector3 roundedDestination = new Vector3(Mathf.Round(destination.x), Mathf.Round(destination.y), destination.z);

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

    private void Death()
    {
        Debug.Log("Frog died! Respawning...");
        StopAllCoroutines();
        isLeaping = false;

        transform.rotation = Quaternion.identity;
        spriteRenderer.sprite = deathSprite;
        enabled = false;

        frogger.linearVelocity = Vector2.zero;
        transform.SetParent(null);

        Invoke(nameof(Respawn), 1f);
    }

    private void Respawn()
    {
        Debug.Log($"Respawning at: {spawnPosition}");
        StopAllCoroutines();
        isLeaping = false;

        transform.rotation = Quaternion.identity;
        transform.position = spawnPosition;
        spriteRenderer.sprite = idleSprite;
        enabled = true;

        // Reset invincibility on respawn
        if (isInvincible)
        {
            EndInvincibility();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;

        // Don't die if invincible
        if (isInvincible) return;

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
        }

        isInvincible = true;
        spriteRenderer.color = invincibilityColor;

        StartCoroutine(FlashEffect(duration));

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
        }

        this.multiplier = multiplier; // ✅ Use the passed multiplier
        isSpeedBoosted = true;
    
        Invoke(nameof(EndSpeedBoost), duration);
    }

    private void EndSpeedBoost()
    {
        isSpeedBoosted = false;
        currentSpeed = normalSpeed;
        multiplier = 1f;
    }

    private IEnumerator FlashEffect(float duration)
    {
        float endTime = Time.time + duration;
        float flashRate = 0.2f;

        while (Time.time < endTime)
        {
            if (spriteRenderer.color.a > 0.5f)
            {
                spriteRenderer.color = new Color(invincibilityColor.r, invincibilityColor.g, invincibilityColor.b, 0.5f);
            }
            else
            {
                spriteRenderer.color = invincibilityColor;
            }

            yield return new WaitForSeconds(flashRate);
        }

        spriteRenderer.color = originalColor;
    }
}
