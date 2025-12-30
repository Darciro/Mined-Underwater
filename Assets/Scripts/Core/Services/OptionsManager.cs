using System;
using UnityEngine;
using UnityEngine.Audio;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager Instance { get; private set; }

    #region PlayerPrefs Keys

    private const string SIMPLE_MOVEMENT_KEY = "option_simple_movement";
    private const string SOUND_FX_KEY = "option_sound_fx";
    private const string MUSIC_KEY = "option_music";
    private const string VIBRATION_KEY = "option_vibration";
    private const string LANGUAGE_KEY = "option_language";

    #endregion

    #region Defaults

    private const bool DEFAULT_SIMPLE_MOVEMENT = true;
    private const float DEFAULT_SOUND_FX = 1f;
    private const float DEFAULT_MUSIC = 1f;
    private const bool DEFAULT_VIBRATION = true;
    private const string DEFAULT_LANGUAGE = "en";

    #endregion

    #region Audio

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;

    private const string MIXER_SFX_PARAM = "SFXVolume";
    private const string MIXER_MUSIC_PARAM = "MusicVolume";

    #endregion

    #region Events

    public event Action<bool> OnSimpleMovementChanged;
    public event Action<float> OnSoundFXChanged;
    public event Action<float> OnMusicChanged;
    public event Action<bool> OnVibrationChanged;
    public event Action<string> OnLanguageChanged;

    #endregion

    #region Editor Debug (Read-Only)

#if UNITY_EDITOR
    [Header("DEBUG – PlayerPrefs (Read Only)")]
    [SerializeField] private bool debugSimpleMovement;
    [SerializeField] private float debugSoundFX;
    [SerializeField] private float debugMusic;
    [SerializeField] private bool debugVibration;
    [SerializeField] private string debugLanguage;
#endif

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAndApplyAll();

#if UNITY_EDITOR
        UpdateDebugValues();
#endif
    }

    #endregion

    #region Load / Apply

    private void LoadAndApplyAll()
    {
        ApplySimpleMovement(GetSimpleMovement());
        ApplySoundFX(GetSoundFX());
        ApplyMusic(GetMusic());
        ApplyVibration(GetVibration());
        ApplyLanguage(GetLanguage());
    }

    #endregion

    #region Simple Movement

    public void SetSimpleMovement(bool value)
    {
        PlayerPrefs.SetInt(SIMPLE_MOVEMENT_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        ApplySimpleMovement(value);
        RefreshDebug();
    }

    public bool GetSimpleMovement()
    {
        return PlayerPrefs.GetInt(
            SIMPLE_MOVEMENT_KEY,
            DEFAULT_SIMPLE_MOVEMENT ? 1 : 0
        ) == 1;
    }

    private void ApplySimpleMovement(bool value)
    {
        OnSimpleMovementChanged?.Invoke(value);
    }

    #endregion

    #region Sound FX

    public void SetSoundFX(float value)
    {
        PlayerPrefs.SetFloat(SOUND_FX_KEY, value);
        PlayerPrefs.Save();

        ApplySoundFX(value);
        RefreshDebug();
    }

    public float GetSoundFX()
    {
        return PlayerPrefs.GetFloat(SOUND_FX_KEY, DEFAULT_SOUND_FX);
    }

    private void ApplySoundFX(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(MIXER_SFX_PARAM, LinearToDb(value));
        }

        OnSoundFXChanged?.Invoke(value);
    }

    #endregion

    #region Music

    public void SetMusic(float value)
    {
        PlayerPrefs.SetFloat(MUSIC_KEY, value);
        PlayerPrefs.Save();

        ApplyMusic(value);
        RefreshDebug();
    }

    public float GetMusic()
    {
        return PlayerPrefs.GetFloat(MUSIC_KEY, DEFAULT_MUSIC);
    }

    private void ApplyMusic(float value)
    {
        if (audioMixer != null)
        {
            audioMixer.SetFloat(MIXER_MUSIC_PARAM, LinearToDb(value));
        }

        OnMusicChanged?.Invoke(value);
    }

    #endregion

    #region Vibration

    public void SetVibration(bool value)
    {
        PlayerPrefs.SetInt(VIBRATION_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        ApplyVibration(value);
        RefreshDebug();
    }

    public bool GetVibration()
    {
        return PlayerPrefs.GetInt(
            VIBRATION_KEY,
            DEFAULT_VIBRATION ? 1 : 0
        ) == 1;
    }

    private void ApplyVibration(bool value)
    {
        OnVibrationChanged?.Invoke(value);
    }

    #endregion

    #region Language

    public void SetLanguage(string languageCode)
    {
        PlayerPrefs.SetString(LANGUAGE_KEY, languageCode);
        PlayerPrefs.Save();

        ApplyLanguage(languageCode);
        RefreshDebug();
    }

    public string GetLanguage()
    {
        return PlayerPrefs.GetString(LANGUAGE_KEY, DEFAULT_LANGUAGE);
    }

    private void ApplyLanguage(string languageCode)
    {
        OnLanguageChanged?.Invoke(languageCode);
    }

    #endregion

    #region Editor Debug Helpers

#if UNITY_EDITOR
    private void RefreshDebug()
    {
        UpdateDebugValues();
    }

    private void UpdateDebugValues()
    {
        debugSimpleMovement = GetSimpleMovement();
        debugSoundFX = GetSoundFX();
        debugMusic = GetMusic();
        debugVibration = GetVibration();
        debugLanguage = GetLanguage();
    }
#else
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void RefreshDebug() { }
#endif

    #endregion

    #region Utils

    private float LinearToDb(float value)
    {
        if (value <= 0.0001f)
            return -80f;

        return Mathf.Log10(value) * 20f;
    }

    #endregion
}