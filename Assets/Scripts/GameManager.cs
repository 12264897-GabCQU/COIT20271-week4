// GameManager.cs
// ─────────────────────────────────────────────────────────────────────────────
// A singleton manager that controls the game session: timer, score, and UI.
// Any script in the scene can call GameManager.Instance.AddScore(n) to
// award points without needing a direct reference to this GameObject.
// ─────────────────────────────────────────────────────────────────────────────

using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    // ── Singleton ────────────────────────────────────────────────────────
    // A static property — belongs to the CLASS, not any instance.
    // Set in Awake so it's available before Start() runs anywhere.
    public static GameManager Instance { get; private set; }

    // ── Inspector References ─────────────────────────────────────────────
    [SerializeField] private TMP_Text  scoreText;
    [SerializeField] private TMP_Text  timerText;
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private TMP_Text  finalScoreText;

    // ── Game Settings ────────────────────────────────────────────────────
    [SerializeField] private float gameDuration = 120f; // 2 minutes

    // ── Runtime State ────────────────────────────────────────────────────
    private int   totalScore = 0;
    private float timeRemaining;
    private bool  gameIsOver = false;

    // ────────────────────────────────────────────────────────────────────
    // Awake: set up the singleton reference.
    // This runs before any Start(), so Instance is ready for everyone.
    // ────────────────────────────────────────────────────────────────────
    void Awake()
    {
        // If an Instance already exists (e.g. from a previous scene load), destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // ────────────────────────────────────────────────────────────────────
    // Start: initialise the game session
    // ────────────────────────────────────────────────────────────────────
    void Start()
    {
        timeRemaining = gameDuration;
        totalScore    = 0;
        gameIsOver    = false;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        UpdateUI();
    }

    // ────────────────────────────────────────────────────────────────────
    // Update: count down the timer each frame
    // ────────────────────────────────────────────────────────────────────
    void Update()
    {
        if (gameIsOver) return;

        timeRemaining -= Time.deltaTime;

        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            EndGame();
        }

        UpdateTimerDisplay();
    }

    // ────────────────────────────────────────────────────────────────────
    // Public API — called by Inventory, Chicken, etc.
    // ────────────────────────────────────────────────────────────────────

    public void AddScore(int points)
    {
        if (gameIsOver) return;
        totalScore += points;
        UpdateScoreDisplay();
        Debug.Log($"Score: {totalScore} (+{points})");
    }

    // ────────────────────────────────────────────────────────────────────
    // UI helpers
    // ────────────────────────────────────────────────────────────────────

    private void UpdateUI()
    {
        UpdateScoreDisplay();
        UpdateTimerDisplay();
    }

    private void UpdateScoreDisplay()
    {
        if (scoreText != null)
            scoreText.text = $"Score: {totalScore}";
    }

    private void UpdateTimerDisplay()
    {
        if (timerText == null) return;

        // Format as MM:SS for readability
        int minutes = (int)(timeRemaining / 60);
        int seconds = (int)(timeRemaining % 60);
        timerText.text = $"{minutes:00}:{seconds:00}";

        // Turn red in the last 30 seconds as a visual warning
        timerText.color = timeRemaining <= 30f
            ? new Color(0.9f, 0.2f, 0.1f)
            : Color.white;
    }

    private void EndGame()
    {
        gameIsOver = true;
        Debug.Log($"Game Over! Final score: {totalScore}");

        if (gameOverPanel != null)
            gameOverPanel.SetActive(true);

        if (finalScoreText != null)
            finalScoreText.text = $"Final Score\n{totalScore}";

        // Pause the game
        Time.timeScale = 0f;
    }
}