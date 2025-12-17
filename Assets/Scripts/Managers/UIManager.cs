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

    private ScoreManager scoreManager;

    private void Start()
    {
        GameVersionSetup();
        UpdatePlayerHealth();
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Update()
    {
        UpdatePlayerHealth();
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
}
