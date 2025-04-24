using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.InputSystem.LowLevel;
using System;

public enum GameState { Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    [Header("Game State")]
    public GameState currentGameState = GameState.Playing;

    [Header("Scene Management")]
    public string mainMenuSceneName = "MainMenu";
    public string gameplaySceneName = "Gameplay";

    [Header("Game References")]
    public PlayerController player;
    public EnemySpawner enemySpawner;
    public GameObject projectileContainer;

    [Header("UI References")]
    public GameObject gameplayPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;

    [Header("UI Text References")]
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;
    public TextMeshProUGUI gameOverHighScoreText;

    public event Action OnPause;
    public event Action OnResume;

    private void Start()
    {
        FindGameReferences(); // If not assigned in inspector

        // Subscribe to player events
        if (player != null)
        {
            player.OnDestroyed += OnPlayerDestroyed;
        }

        // Subscribe to score manager events
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged += UpdateScoreUI;
            ScoreManager.Instance.OnHighScoreChanged += UpdateHighScoreUI;
        }

        // Initial state
        SetGameState(GameState.Playing);

        // Initialize UI
        UpdateAllScoreUI();
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (player != null)
        {
            player.OnDestroyed -= OnPlayerDestroyed;
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.OnScoreChanged -= UpdateScoreUI;
            ScoreManager.Instance.OnHighScoreChanged -= UpdateHighScoreUI;
        }
    }

    private void FindGameReferences()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.GetComponent<PlayerController>();
            }
        }

        if (enemySpawner == null)
        {
            enemySpawner = FindFirstObjectByType<EnemySpawner>();
        }

        if(projectileContainer == null)
        {
            projectileContainer = GameObject.Find("ProjectileContainer");
        }

        if (gameplayPanel == null) gameplayPanel = GameObject.Find("GameplayPanel");
        if (pauseMenuPanel == null) pauseMenuPanel = GameObject.Find("PauseMenuPanel");
        if (gameOverPanel == null) gameOverPanel = GameObject.Find("GameOverPanel");

        if (scoreText == null) scoreText = GameObject.Find("ScoreText")?.GetComponent<TextMeshProUGUI>();
        if (highScoreText == null) highScoreText = GameObject.Find("HighScoreText")?.GetComponent<TextMeshProUGUI>();
        if (gameOverHighScoreText == null) gameOverHighScoreText = GameObject.Find("GameOverHighScoreText")?.GetComponent<TextMeshProUGUI>();
    }

    public void SetGameState(GameState newState)
    {
        // Exit current state
        switch (currentGameState)
        {
            case GameState.Playing:
                if (gameplayPanel != null) gameplayPanel.SetActive(false);
                break;
            case GameState.Paused:
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
                Time.timeScale = 1f;
                break;
            case GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(false);
                break;
        }

        // Enter new state
        currentGameState = newState;
        switch (newState)
        {
            case GameState.Playing:
                if (gameplayPanel != null) gameplayPanel.SetActive(true);
                if (enemySpawner != null) enemySpawner.StartSpawning();
                Time.timeScale = 1f;
                OnResume?.Invoke();
                if (projectileContainer != null) projectileContainer.SetActive(true);
                break;
            case GameState.Paused:
                if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
                if (enemySpawner != null) enemySpawner.StopSpawning();
                OnPause?.Invoke();
                Time.timeScale = 0f;
                if (projectileContainer != null) projectileContainer.SetActive(false);
                break;
            case GameState.GameOver:
                if (gameOverPanel != null) gameOverPanel.SetActive(true);
                if (enemySpawner != null) enemySpawner.StopSpawning();
                OnPause?.Invoke();
                UpdateGameOverUI();
                if (projectileContainer != null) projectileContainer.SetActive(false);
                break;
        }
    }

    public void PauseGame()
    {
        if (currentGameState == GameState.Playing)
        {
            SetGameState(GameState.Paused);
        }
    }

    public void ResumeGame()
    {
        if (currentGameState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }

    public void LoadMainMenu()
    {
        Time.timeScale = 1f;
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void StartNewGame()
    {
        // Reset score before starting
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.ResetScore();
        }
        SceneManager.LoadScene(gameplaySceneName);
    }

    public void RestartGame()
    {
        StartNewGame();
    }

    public void GameOver()
    {
        SetGameState(GameState.GameOver);
    }

    // Called when the player is destroyed
    private void OnPlayerDestroyed(GameObject playerObject)
    {
        GameOver();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    private void UpdateScoreUI(int newScore)
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + newScore;
        }
    }

    private void UpdateHighScoreUI(int newHighScore)
    {
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + newHighScore;
        }

        if (gameOverHighScoreText != null)
        {
            gameOverHighScoreText.text = "High Score: " + newHighScore;
        }
    }

    private void UpdateAllScoreUI()
    {
        if (ScoreManager.Instance != null)
        {
            UpdateScoreUI(ScoreManager.Instance.currentScore);
            UpdateHighScoreUI(ScoreManager.Instance.highScore);
        }
    }

    private void UpdateGameOverUI()
    {
        if (gameOverHighScoreText != null && ScoreManager.Instance != null)
        {
            gameOverHighScoreText.text = "High Score: " + ScoreManager.Instance.highScore;
        }
    }
}