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

        if (barrier != null)
        {
            if (transform.parent != null)
            {
                transform.SetParent(null);
                if (!isInvincible)
                {
                    Death();
                }
                else
                {
                    // Allow movement through barriers when invincible
                    StartCoroutine(Leap(destination));
                }
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

        // Only die if not invincible
        if (!isInvincible)
        {
            if (obstacle != null && platform == null)
            {
                transform.position = destination;
                Death();
                return;
            }
            else if (environment != null && platform == null)
            {
                transform.position = destination;
                Death();
                return;
            }
        }

        // If we reach here, the move is valid or we're invincible
        StartCoroutine(Leap(destination));
    }

    private IEnumerator Leap(Vector3 destination)
    {
        isLeaping = true;
        Vector3 startPosition = transform.position;
        Vector3 roundedDestination;

        if (transform.parent != null)
        {
            roundedDestination = new Vector3(destination.x, Mathf.Round(destination.y), destination.z);
        }
        else
        {
            roundedDestination = new Vector3(Mathf.Round(destination.x), Mathf.Round(destination.y), destination.z);
        }

        float elapsed = 0f;
        float duration = 0.125f;

        spriteRenderer.sprite = leapSprite;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
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
        StopAllCoroutines();
        isLeaping = false;

        transform.rotation = Quaternion.identity;
        transform.position = spawnPosition;
        spriteRenderer.sprite = idleSprite;
        enabled = true;

        // Reset invincibility visuals (just in case)
        spriteRenderer.color = originalColor;

        // 👇 Grant 5 seconds of safety after respawn
        ActivateInvincibility(5f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled) return;

        // If invincible, don't die from collisions
        if (isInvincible) return;

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
        // Use default duration if none provided
        if (duration <= 0f)
        {
            duration = invincibilityDuration;
        }

        // If already invincible, just reset the timer
        if (isInvincible)
        {
            CancelInvoke(nameof(EndInvincibility));
        }

        isInvincible = true;

        // Visual indicator for invincibility
        spriteRenderer.color = invincibilityColor;

        // Flash effect to show invincibility
        StartCoroutine(FlashEffect(duration));

        // Set timer to end invincibility
        Invoke(nameof(EndInvincibility), duration);
    }

    private void EndInvincibility()
    {
        isInvincible = false;
        StopCoroutine(nameof(FlashEffect));
        spriteRenderer.color = originalColor;
    }

    // Create a flashing effect to indicate invincibility
    private IEnumerator FlashEffect(float duration)
    {
        float endTime = Time.time + duration;
        float flashRate = 0.2f; // How fast to flash

        while (Time.time < endTime)
        {
            // Toggle between invincibility color and half-transparent
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

        // Ensure color is reset at the end
        spriteRenderer.color = originalColor;
    }
}