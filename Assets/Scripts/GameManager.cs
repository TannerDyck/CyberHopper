using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    private Frogger frogger;
    private Home[] homes;
    private int score;
    private int lives;
    private bool gameOver;
    private float timeRemaining;
    private const float ROUND_TIME = 30f;

    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI livesText;
    [SerializeField] private TextMeshProUGUI timerText;

    private void Awake()
    {
        homes = FindObjectsOfType<Home>();
        frogger = FindObjectOfType<Frogger>();

        // Find UI text components if not assigned
        if (scoreText == null)
        {
            scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        }
        if (livesText == null)
        {
            livesText = GameObject.Find("LivesText")?.GetComponent<TextMeshProUGUI>();
        }
        if (timerText == null)
        {
            timerText = GameObject.Find("TimerText")?.GetComponent<TextMeshProUGUI>();
        }

        NewGame();
    }

    private void NewGame()
    {
        gameOver = false;
        SetScore(0);
        SetLives(3);
        NewLevel();
    }

    private void NewLevel()
    {
        for (int i = 0; i < homes.Length; i++)
        {
            homes[i].enabled = false;
        }

        frogger.gameObject.SetActive(true);
        NewRound();
    }

    private void NewRound()
    {
        if (!gameOver)
        {
            StopAllCoroutines();
            StartCoroutine(TimerCoroutine());
            frogger.gameObject.SetActive(true);
            frogger.Respawn();
        }
    }

    private IEnumerator TimerCoroutine()
    {
        timeRemaining = ROUND_TIME;

        while (timeRemaining > 0)
        {
            // Update UI timer text
            if (timerText != null)
            {
                timerText.text = Mathf.CeilToInt(timeRemaining).ToString();
            }

            yield return new WaitForSeconds(1f);
            timeRemaining--;

            if (timeRemaining <= 0)
            {
                frogger.Death();
                break;
            }
        }
    }

    public void AdvancedRow()
    {
        SetScore(score + 10);
    }

    public void HomeOccupied()
    {
        frogger.gameObject.SetActive(false);
        StopAllCoroutines();

        // Calculate and add time bonus and base points for reaching home
        int timeBonus = Mathf.RoundToInt(timeRemaining * 20);
        SetScore(score + timeBonus + 50);

        if (Cleared())
        {
            // Add clearing bonus separately to avoid overwriting previous score
            SetScore(score + 1000);
            Invoke(nameof(NewLevel), 1f);
        }
        else
        {
            Invoke(nameof(NewRound), 1f);
        }
    }

    public void FroggerDied()
    {
        StopAllCoroutines();
        SetLives(lives - 1);

        if (lives > 0)
        {
            Invoke(nameof(NewRound), 1f);
        }
        else
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        gameOver = true;
        StopAllCoroutines();

        // Wait 1 second to show death sprite before deactivating Frogger
        Invoke(nameof(DeactivateFrogger), 1f);
    }

    private void DeactivateFrogger()
    {
        frogger.gameObject.SetActive(false);
        // Wait 1 more second before restarting
        Invoke(nameof(NewGame), 1f);
    }

    private bool Cleared()
    {
        for (int i = 0; i < homes.Length; i++)
        {
            if (!homes[i].enabled) return false;
        }
        return true;
    }

    private void SetScore(int newScore)
    {
        this.score = newScore;
        // Update UI score text with leading zeros
        if (scoreText != null)
        {
            scoreText.text = score.ToString("D4");
        }
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        // Update UI lives text
        if (livesText != null)
        {
            livesText.text = lives.ToString();
        }
    }

    public float GetTimeRemaining()
    {
        return timeRemaining;
    }
}
