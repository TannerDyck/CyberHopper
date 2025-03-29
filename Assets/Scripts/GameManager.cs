using UnityEngine;

public class GameManager : MonoBehaviour
{
    private Frogger frogger;
    private Home[] homes;
    private int score;
    private int lives;

    private void Awake()
    {
        homes = FindObjectsOfType<Home>();
        frogger = FindObjectOfType<Frogger>();
    }

    private void NewGame()
    {
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
        frogger.gameObject.SetActive(true);
        frogger.Respawn();
    }

    public void HomeOccupied()
    {
        frogger.gameObject.SetActive(false);

        if (Cleared())
        {
            SetScore(score + 50); // Bonus for clearing all homes
            Invoke(nameof(NewLevel), 1f);
        }
        else
        {
            SetScore(score + 10); // Points for reaching a home
            Invoke(nameof(NewRound), 1f);
        }
    }

    private bool Cleared()
    {
        for (int i = 0; i < homes.Length; i++)
        {
            if (!homes[i].enabled) return false;
        }
        return true;
    }

    private void SetScore(int score)
    {
        this.score = score;
        // ... update UI
    }

    private void SetLives(int lives)
    {
        this.lives = lives;
        // ... update UI
    }
}
