using System.Collections;
using Febucci.TextAnimatorForUnity.TextMeshPro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gameVersionText;
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Slider playerHealthSlider;
    [SerializeField] private TextMeshProUGUI playerHealthtext;
    [SerializeField] private Slider airSlider;
    [SerializeField] private Slider airSliderAlt;
    [SerializeField] private TextMeshProUGUI airText;
    [SerializeField] private TextMeshProUGUI eggsText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("Win Scene Stats")]
    [SerializeField] private TextMeshProUGUI levelNumberText;
    [SerializeField] private TextMeshProUGUI levelEggsText;
    [SerializeField] private TextMeshProUGUI levelCoinsText;
    [SerializeField] private TextMeshProUGUI totalEggsText;
    [SerializeField] private TextMeshProUGUI totalCoinsText;
    [SerializeField] private TextMeshProUGUI eggRequirementText;

    [Header("UI Animations")]
    [Tooltip("Text Animator style tag (must exist in a Text Animator StyleSheet, e.g. 'score', 'egg')")]
    [SerializeField] private string animationTag = "score";

    [Header("Panels")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject settingsPanel;

    [Header("SFX UI")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private ScoreManager scoreManager;

    private int previousEggs = 0;
    private int previousCoins = 0;

    private Coroutine eggsAnimationCoroutine;
    private Coroutine coinsAnimationCoroutine;

    private TextAnimator_TMP eggsTextAnimator;
    private TextAnimator_TMP coinsTextAnimator;

    private void Awake()
    {
        scoreManager = FindFirstObjectByType<ScoreManager>();
    }

    private void Start()
    {
        GameVersionSetup();
        SetPaused(false);

        if (scoreManager != null)
        {
            scoreManager.ResetEggs();
            scoreManager.ResetCoins();
        }

        previousEggs = 0;
        previousCoins = 0;

        InitializeTextAnimators();
        InitializeOptionsUI();
        UpdatePlayerHealth();
        RefreshEggs(true);
        RefreshCoins(true);
        DisplayWinSceneStats();

        // Subscribe to game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        }
    }

    private void Update()
    {
        UpdatePlayerHealth();
        UpdatePlayerAir();
        RefreshEggs(false);
        RefreshCoins(false);
    }

    #region Initialization

    private void InitializeTextAnimators()
    {
        if (eggsText != null)
        {
            eggsTextAnimator = eggsText.GetComponent<TextAnimator_TMP>();
            if (eggsTextAnimator == null)
                eggsTextAnimator = eggsText.gameObject.AddComponent<TextAnimator_TMP>();
        }

        if (coinsText != null)
        {
            coinsTextAnimator = coinsText.GetComponent<TextAnimator_TMP>();
            if (coinsTextAnimator == null)
                coinsTextAnimator = coinsText.gameObject.AddComponent<TextAnimator_TMP>();
        }
    }

    private void InitializeOptionsUI()
    {
        var options = OptionsManager.Instance;
        if (options == null) return;

        // Set slider values to saved preferences
        if (musicSlider != null)
            musicSlider.value = options.GetMusic();

        if (sfxSlider != null)
            sfxSlider.value = options.GetSoundFX();

        // Subscribe to changes (optional, if you want real-time updates)
        options.OnMusicChanged += UpdateMusicSlider;
        options.OnSoundFXChanged += UpdateSFXSlider;
    }

    private void OnDestroy()
    {
        var options = OptionsManager.Instance;
        if (options != null)
        {
            options.OnMusicChanged -= UpdateMusicSlider;
            options.OnSoundFXChanged -= UpdateSFXSlider;
        }

        // Unsubscribe from game state changes
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        }
    }

    private void UpdateMusicSlider(float value)
    {
        if (musicSlider != null)
            musicSlider.value = value;
    }

    private void UpdateSFXSlider(float value)
    {
        if (sfxSlider != null)
            sfxSlider.value = value;
    }

    #endregion

    #region Health UI

    private void UpdatePlayerHealth()
    {
        if (playerController == null) return;

        if (playerHealthSlider != null)
        {
            playerHealthSlider.value =
                (float)playerController.GetHealth() / playerController.GetMaxHealth();
        }

        if (playerHealthtext != null)
        {
            int currentHealth = Mathf.Max(0, playerController.GetHealth());
            playerHealthtext.text =
                $"{currentHealth} / {playerController.GetMaxHealth()}";
        }
    }

    #endregion

    #region Air UI

    private void UpdatePlayerAir()
    {
        if (playerController == null) return;

        float airValue = (float)playerController.GetAir() / playerController.GetMaxAir();

        if (airSlider != null)
        {
            airSlider.value = airValue;
        }

        if (airSliderAlt != null)
        {
            airSliderAlt.value = airValue;
        }

        if (airText != null)
        {
            int currentAir = Mathf.Max(0, playerController.GetAir());
            airText.text =
                $"{currentAir} / {playerController.GetMaxAir()}";
        }
    }

    #endregion

    #region Eggs UI

    private void RefreshEggs(bool force)
    {
        if (scoreManager == null || eggsTextAnimator == null) return;

        int currentEggs = scoreManager.GetEggsCollected();

        // Animate only on increment
        if (currentEggs > previousEggs)
        {
            // Stop any existing animation coroutine
            if (eggsAnimationCoroutine != null)
                StopCoroutine(eggsAnimationCoroutine);

            eggsTextAnimator.SetText($"<pulse>{currentEggs}</pulse>");
            previousEggs = currentEggs;

            // Start coroutine to set static text after animation completes
            eggsAnimationCoroutine = StartCoroutine(SetStaticEggsAfterDelay(currentEggs, .75f));
        }
        else if (force)
        {
            // Static text on force initialization
            eggsTextAnimator.SetText(currentEggs.ToString());
            previousEggs = currentEggs;
        }
    }

    private IEnumerator SetStaticEggsAfterDelay(int eggs, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (eggsTextAnimator != null)
            eggsTextAnimator.SetText(eggs.ToString());
    }

    #endregion

    #region Coins UI

    private void RefreshCoins(bool force)
    {
        if (scoreManager == null || coinsTextAnimator == null) return;

        int currentCoins = scoreManager.GetCoinsCollected();

        // Animate only on increment
        if (currentCoins > previousCoins)
        {
            // Stop any existing animation coroutine
            if (coinsAnimationCoroutine != null)
                StopCoroutine(coinsAnimationCoroutine);

            coinsTextAnimator.SetText($"<pulse>{currentCoins}</pulse>");
            previousCoins = currentCoins;

            // Start coroutine to set static text after animation completes
            coinsAnimationCoroutine = StartCoroutine(SetStaticCoinsAfterDelay(currentCoins, .75f));
        }
        else if (force)
        {
            // Static text on force initialization
            coinsTextAnimator.SetText(currentCoins.ToString());
            previousCoins = currentCoins;
        }
    }

    private IEnumerator SetStaticCoinsAfterDelay(int coins, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (coinsTextAnimator != null)
            coinsTextAnimator.SetText(coins.ToString());
    }

    #endregion

    #region Win Scene Stats

    /// <summary>
    /// Displays level completion statistics on the Win scene
    /// </summary>
    private void DisplayWinSceneStats()
    {
        if (GameManager.Instance == null) return;

        int completedLevel = GameManager.Instance.CurrentLevel;
        int levelEggs = GameManager.Instance.LevelEggs;
        int levelCoins = GameManager.Instance.LevelCoins;
        int totalEggs = GameManager.Instance.TotalEggs;
        int totalCoins = GameManager.Instance.TotalCoins;
        int eggRequirement = GameManager.Instance.GetEggRequirement();

        // Display level number
        if (levelNumberText != null)
            levelNumberText.text = $"Level {completedLevel}";

        // Display level stats
        if (levelEggsText != null)
            levelEggsText.text = $"{levelEggs}";

        if (levelCoinsText != null)
            levelCoinsText.text = $"{levelCoins}";

        // Display total stats
        if (totalEggsText != null)
            totalEggsText.text = $"{totalEggs}";

        if (totalCoinsText != null)
            totalCoinsText.text = $"{totalCoins}";

        // Display egg requirement (for reference)
        if (eggRequirementText != null)
            eggRequirementText.text = $"x {eggRequirement}";
    }

    /// <summary>
    /// Public method to manually refresh win scene stats (can be called from buttons)
    /// </summary>
    public void RefreshWinSceneStats()
    {
        DisplayWinSceneStats();
    }

    #endregion

    #region Game Version

    private void GameVersionSetup()
    {
        if (gameVersionText != null)
            gameVersionText.text = $"Version {Application.version}";
    }

    #endregion

    #region Pause

    public void TogglePause()
    {
        if (GameManager.Instance == null) return;

        bool isPaused = GameManager.Instance.CurrentState == GameStateEnum.Paused;
        SetPaused(!isPaused);
    }

    private void SetPaused(bool pause)
    {
        if (GameManager.Instance == null)
        {
            // Fallback if GameManager is not available
            Time.timeScale = pause ? 0f : 1f;
            if (pausePanel != null)
                pausePanel.SetActive(pause);
            return;
        }

        // Check if already in the desired state
        bool currentlyPaused = GameManager.Instance.CurrentState == GameStateEnum.Paused;
        if (currentlyPaused == pause) return;

        // Use GameManager to handle state changes and time scale
        GameManager.Instance.ChangeState(pause ? GameStateEnum.Paused : GameStateEnum.Playing);
    }

    private void HandleGameStateChanged(GameStateEnum newState)
    {
        // Update pause panel visibility based on state
        if (pausePanel != null)
        {
            pausePanel.SetActive(newState == GameStateEnum.Paused);
        }
    }

    #endregion

    #region Settings

    public void OpenSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void SimpleMovementToggle()
    {
        var options = OptionsManager.Instance;
        if (options == null) return;
        options.SetSimpleMovement(!options.GetSimpleMovement());
    }

    public void VibrationToggle()
    {
        var options = OptionsManager.Instance;
        if (options == null) return;
        options.SetVibration(!options.GetVibration());
    }

    public void SetMusicVolume()
    {
        var options = OptionsManager.Instance;
        if (options == null) return;
        options.SetMusic(musicSlider.value);
    }

    public void SetSFXVolume()
    {
        var options = OptionsManager.Instance;
        if (options == null) return;
        options.SetSoundFX(sfxSlider.value);
    }
    #endregion
}
