using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    // Singleton pattern
    public static ScoreManager Instance { get; private set; }

    [Header("Score Settings")]
    public int currentScore = 0;
    public int highScore = 0;

    // Event that other scripts can subscribe to
    public delegate void ScoreChangedEvent(int newScore);
    public event ScoreChangedEvent OnScoreChanged;

    // Event for high score changes
    public delegate void HighScoreChangedEvent(int newHighScore);
    public event HighScoreChangedEvent OnHighScoreChanged;

    private void Awake()
    {
        // Singleton setup with persistence between scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Load high score from player preferences
            highScore = PlayerPrefs.GetInt("HighScore", 0);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddScore(int points)
    {
        currentScore += points;

        // Notify listeners that score has changed
        OnScoreChanged?.Invoke(currentScore);

        // Check for new high score
        if (currentScore > highScore)
        {
            highScore = currentScore;
            SaveHighScore();
            OnHighScoreChanged?.Invoke(highScore);
        }
    }

    public void ResetScore()
    {
        currentScore = 0;
        OnScoreChanged?.Invoke(currentScore);
    }

    public void SaveHighScore()
    {
        PlayerPrefs.SetInt("HighScore", highScore);
        PlayerPrefs.Save();
    }

    // Helper method for scoring based on enemy type
    public void ScoreFromEnemyDestruction(GameObject enemy)
    {
        Enemy enemyComponent = enemy.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            if (enemyComponent.enemyType == EnemyType.Shooter)
            {
                AddScore(200);
            }
            else if (enemyComponent.enemyType == EnemyType.Melee)
            {
                AddScore(100);
            }
        }
        else
        {
            // Default score if can't determine type
            AddScore(50);
        }
    }
}