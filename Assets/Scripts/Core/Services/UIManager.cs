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
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI eggsText;

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

    private int previousScore = 0;
    private int previousEggs = 0;

    private Coroutine scoreAnimationCoroutine;
    private Coroutine eggsAnimationCoroutine;

    private TextAnimator_TMP scoreTextAnimator;
    private TextAnimator_TMP eggsTextAnimator;

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
            scoreManager.ResetScore();
            scoreManager.ResetEggs();
        }

        previousScore = 0;
        previousEggs = 0;

        InitializeTextAnimators();
        InitializeOptionsUI();
        UpdatePlayerHealth();
        RefreshScore(true);
        RefreshEggs(true);

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
        RefreshScore(false);
        RefreshEggs(false);
    }

    #region Initialization

    private void InitializeTextAnimators()
    {
        if (scoreText != null)
        {
            scoreTextAnimator = scoreText.GetComponent<TextAnimator_TMP>();
            if (scoreTextAnimator == null)
                scoreTextAnimator = scoreText.gameObject.AddComponent<TextAnimator_TMP>();
        }

        if (eggsText != null)
        {
            eggsTextAnimator = eggsText.GetComponent<TextAnimator_TMP>();
            if (eggsTextAnimator == null)
                eggsTextAnimator = eggsText.gameObject.AddComponent<TextAnimator_TMP>();
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

    #region Score UI

    private void RefreshScore(bool force)
    {
        if (scoreManager == null || scoreTextAnimator == null) return;

        int currentScore = scoreManager.GetScore();

        // Animate only on increment
        if (currentScore > previousScore)
        {
            // Stop any existing animation coroutine
            if (scoreAnimationCoroutine != null)
                StopCoroutine(scoreAnimationCoroutine);

            scoreTextAnimator.SetText($"<pulse>{currentScore}</pulse>");
            previousScore = currentScore;

            // Start coroutine to set static text after animation completes
            scoreAnimationCoroutine = StartCoroutine(SetStaticScoreAfterDelay(currentScore, .75f));
        }
        else if (force)
        {
            // Static text on force initialization
            scoreTextAnimator.SetText(currentScore.ToString());
            previousScore = currentScore;
        }
    }

    private IEnumerator SetStaticScoreAfterDelay(int score, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (scoreTextAnimator != null)
            scoreTextAnimator.SetText(score.ToString());
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
