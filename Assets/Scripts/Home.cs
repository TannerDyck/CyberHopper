using UnityEngine;

public class Home : MonoBehaviour
{
    public GameObject frog;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found!");
        }
    }

    private void OnEnable()
    {
        if (frog != null)
        {
            frog.SetActive(true);
        }
    }

    private void OnDisable()
    {
        if (frog != null)
        {
            frog.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!enabled && other.gameObject.CompareTag("Player"))
        {
            enabled = true;
            if (gameManager != null)
            {
                gameManager.HomeOccupied();
            }
            else
            {
                // Fallback in case gameManager reference is lost
                FindObjectOfType<GameManager>()?.HomeOccupied();
            }
        }
    }
}