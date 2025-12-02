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

    private ScoreManager scoreManager;

    private void Start()
    {
        GameVersionSetup();
        UpdatePlayerHealth();

        /* scoreManager = FindFirstObjectByType<ScoreManager>();

        if (playerController != null && playerHealthSlider != null)
        {
            playerHealthSlider.maxValue = playerController.GetMaxHealth();
            playerHealthSlider.value = playerController.GetHealth();
        } */


    }

    private void Update()
    {
        UpdatePlayerHealth();
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
}
