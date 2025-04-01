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
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject youWinPanel;
    [SerializeField] private TextMeshProUGUI gameOverScoreText;
    [SerializeField] private TextMeshProUGUI gameWinScoreText;

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
        if (gameOverPanel == null)
        {
            gameOverPanel = GameObject.Find("GameOverPanel");
        }
        if (youWinPanel == null)
        {
            youWinPanel = GameObject.Find("YouWinPanel");
        }

        // Ensure panels exist and set initial visibility
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("GameOverPanel not found in the scene!");
        }

        if (youWinPanel != null)
        {
            youWinPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("YouWinPanel not found in the scene!");
        }

        NewGame();
    }

    private void NewGame()
    {
        gameOver = false;
        SetScore(0);
        SetLives(3);
        Time.timeScale = 1;

        // Ensure panels are hidden when starting new game
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (youWinPanel != null) youWinPanel.SetActive(false);

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
            // Show win panel and stop the game
            if (youWinPanel != null) youWinPanel.SetActive(true);

            // Show the final score
            if (gameWinScoreText != null) 
            {
                gameWinScoreText.text = score.ToString("D4");
            }

            gameOver = true; // Prevent further game updates
            Time.timeScale = 0; // Pause the game
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

        // Show game over panel
        if (gameOverPanel != null) gameOverPanel.SetActive(true);

        // Show the final score
        if (gameOverScoreText != null) 
        {
            gameOverScoreText.text = "" + score.ToString("D4");
        }

        frogger.gameObject.SetActive(false);
        Time.timeScale = 0; // Pause the game
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
