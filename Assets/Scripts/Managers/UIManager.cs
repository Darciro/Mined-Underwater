using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI gameVersionText;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Slider playerHealthSlider;

    [SerializeField] private TextMeshProUGUI scoreText;
    private ScoreManager scoreManager;
    void Start()
    {
        if (gameVersionText != null)
        {
            gameVersionText.text = $"Version {Application.version}";
        }

        if (scoreManager != null && playerController != null)
        {
            scoreManager = FindFirstObjectByType<ScoreManager>();
            playerHealthSlider.maxValue = playerController.GetMaxHealth();
        }
    }

    void Update()
    {
        if (scoreManager != null && playerController != null)
        {
            scoreText.text = scoreManager.GetScore().ToString();
            playerHealthSlider.value = playerController.GetHealth();
        }

    }
}
