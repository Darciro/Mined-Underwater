using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameVersionText;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TextMeshProUGUI playerHealthtext;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI eggsText;
    [Header("Pause")]
    [SerializeField] private GameObject pausePanel; // Optional UI overlay to show when paused

    private ScoreManager scoreManager;
    private bool isPaused = false;

    private void Start()
    {
        GameVersionSetup();
        UpdatePlayerHealth();
        scoreManager = FindFirstObjectByType<ScoreManager>();
        SetPaused(false);
    }

    private void Update()
    {
        UpdatePlayerHealth();
        UpdateScore();
        UpdateEggsCounter();
    }

    private void GameVersionSetup()
    {
        if (gameVersionText != null) gameVersionText.text = $"Version {Application.version}";
    }

    private void UpdatePlayerHealth()
    {
        if (playerHealthSlider != null)
        {
            playerHealthSlider.value = (float)playerController.GetHealth() / playerController.GetMaxHealth();
        }

        if (playerHealthtext != null)
        {
            int currentHealth = playerController.GetHealth();
            if (currentHealth < 0) currentHealth = 0;

            playerHealthtext.text = $"{currentHealth} / {playerController.GetMaxHealth()}";
        }
    }

    private void UpdateEggsCounter()
    {
        if (eggsText != null && scoreManager != null)
        {
            eggsText.text = scoreManager.GetEggsCollected().ToString();
        }
    }

    private void UpdateScore()
    {
        if (scoreText != null && scoreManager != null)
        {
            scoreText.text = scoreManager.GetScore().ToString();
        }
    }

    // Called by the Pause/Resume UI button
    public void TogglePause()
    {
        SetPaused(!isPaused);
    }

    private void SetPaused(bool pause)
    {
        if (isPaused == pause) return;
        isPaused = pause;

        Time.timeScale = pause ? 0f : 1f;

        if (pausePanel != null)
        {
            pausePanel.SetActive(pause);
        }
    }
}
