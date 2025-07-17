using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject mainMenuPanel;
    public GameObject gameUIPanel;
    public GameObject pauseMenuPanel;
    public GameObject gameOverPanel;
    // public GameObject levelUpPanel;

    [Header("Game UI Elements")]
    public Slider healthBar;
    public Slider xpBar;           // ✅ Hanya UIManager yang punya referensi ini
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI killCountText;
    public TextMeshProUGUI waveText;

    [Header("Game Over Elements")]
    public TextMeshProUGUI finalTimeText;
    public TextMeshProUGUI finalKillsText;
    public TextMeshProUGUI finalLevelText;
    public Button restartButton;
    public Button mainMenuButton;

    [Header("Pause Menu")]
    public Button resumeButton;
    public Button pauseMenuButton;
    public Button quitButton;

    [Header("Settings")]
    public bool showFPS = true;
    public TextMeshProUGUI fpsText;

    // Game state
    private bool isPaused = false;
    private float gameStartTime;
    private int killCount = 0;
    private float deltaTime = 0f;

    // References
    private PlayerHealth playerHealth;
    private ExperienceManager experienceManager;
    private EnemySpawner enemySpawner;

    void Start()
    {
        gameStartTime = Time.time;

        // Find references
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
            experienceManager = player.GetComponent<ExperienceManager>();
        }

        enemySpawner = FindObjectOfType<EnemySpawner>();

        // Setup button listeners
        SetupButtonListeners();

        // Subscribe to events
        SetupEventListeners();

        // Start with game UI
        ShowGameUI();
    }

    void SetupEventListeners()
    {
        if (experienceManager != null)
        {
            // ✅ Listen ke events dari ExperienceManager
            // experienceManager.OnLevelUp += OnPlayerLevelUp;
            experienceManager.OnXPProgressChanged += UpdateXPBar;
            experienceManager.OnLevelChanged += UpdateLevelText;
        }
    }

    void Update()
    {
        // Handle pause input
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        // Update UI elements
        UpdateGameUI();

        // Update FPS counter
        if (showFPS && fpsText != null)
        {
            UpdateFPS();
        }
    }

    void SetupButtonListeners()
    {
        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (mainMenuButton != null)
            mainMenuButton.onClick.AddListener(ShowMainMenu);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (pauseMenuButton != null)
            pauseMenuButton.onClick.AddListener(ShowMainMenu);

        if (quitButton != null)
            quitButton.onClick.AddListener(QuitGame);
    }

    void UpdateGameUI()
    {
        // Update health bar
        if (healthBar != null && playerHealth != null)
        {
            healthBar.value = playerHealth.GetCurrentHealth() / playerHealth.GetMaxHealth();
        }

        // ✅ XP bar dan level text diupdate via events, tidak perlu di sini lagi

        // Update timer
        if (timerText != null)
        {
            float gameTime = Time.time - gameStartTime;
            timerText.text = FormatTime(gameTime);
        }

        // Update kill count
        if (killCountText != null)
        {
            killCountText.text = $"Kills: {killCount}";
        }

        // Update wave info
        if (waveText != null && enemySpawner != null)
        {
            waveText.text = $"Wave {enemySpawner.GetCurrentWave()}";
        }
    }

    // ✅ Event handlers untuk XP system
    void UpdateXPBar(float progress)
    {
        if (xpBar != null)
        {
            xpBar.value = progress;
        }
    }

    void UpdateLevelText(int level)
    {
        if (levelText != null)
        {
            levelText.text = $"Level {level}";
        }
    }

    void UpdateFPS()
    {
        deltaTime += (Time.unscaledDeltaTime - deltaTime) * 0.1f;
        float fps = 1.0f / deltaTime;
        fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // UI State Management
    public void ShowMainMenu()
    {
        SetPanelActive(mainMenuPanel);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ShowGameUI()
    {
        SetPanelActive(gameUIPanel);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void PauseGame()
    {
        SetPanelActive(pauseMenuPanel);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void ResumeGame()
    {
        SetPanelActive(gameUIPanel);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void ShowGameOver()
    {
        SetPanelActive(gameOverPanel);
        Time.timeScale = 0f;

        // Update final stats
        if (finalTimeText != null)
        {
            float gameTime = Time.time - gameStartTime;
            finalTimeText.text = $"Time Survived: {FormatTime(gameTime)}";
        }

        if (finalKillsText != null)
            finalKillsText.text = $"Enemies Killed: {killCount}";

        if (finalLevelText != null && experienceManager != null)
            finalLevelText.text = $"Level Reached: {experienceManager.GetCurrentLevel()}";
    }

    // public void ShowLevelUp()
    // {
    //     SetPanelActive(levelUpPanel);
    //     Time.timeScale = 0f;
    // }

    public void HideLevelUp()
    {
        SetPanelActive(gameUIPanel);
        Time.timeScale = 1f;
    }

    void SetPanelActive(GameObject activePanel)
    {
        // Hide all panels
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameUIPanel != null) gameUIPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        // if (levelUpPanel != null) levelUpPanel.SetActive(false);

        // Show target panel
        if (activePanel != null)
            activePanel.SetActive(true);
    }

    // Event handlers
    // void OnPlayerLevelUp(int newLevel)
    // {
    //     ShowLevelUp();
    // }

    public void OnEnemyKilled()
    {
        killCount++;
    }

    public void OnPlayerDied()
    {
        ShowGameOver();
    }

    // Button actions
    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    // Public methods for external use
    public void AddKill()
    {
        killCount++;
    }

    public bool IsPaused()
    {
        return isPaused;
    }

    void OnDestroy()
    {
        // ✅ Unsubscribe dari events untuk mencegah memory leak
        if (experienceManager != null)
        {
            // experienceManager.OnLevelUp -= OnPlayerLevelUp;
            experienceManager.OnXPProgressChanged -= UpdateXPBar;
            experienceManager.OnLevelChanged -= UpdateLevelText;
        }
    }
}