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

    #region Audio (Optional)

    [Header("Audio")]
    [SerializeField] private AudioMixer audioMixer;
    private const string MIXER_SFX_PARAM = "SFXVolume";
    private const string MIXER_MUSIC_PARAM = "MusicVolume";

    #endregion

    #region Events (Optional but Recommended)

    public event Action<bool> OnSimpleMovementChanged;
    public event Action<float> OnSoundFXChanged;
    public event Action<float> OnMusicChanged;
    public event Action<bool> OnVibrationChanged;
    public event Action<string> OnLanguageChanged;

    #endregion

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
    }

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
        Debug.Log("PlayerController: Simple Movement set to " + value);
        PlayerPrefs.SetInt(SIMPLE_MOVEMENT_KEY, value ? 1 : 0);
        PlayerPrefs.Save();

        ApplySimpleMovement(value);
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
        // PlayerController or InputManager should listen to this event
    }

    #endregion

    #region Sound FX

    public void SetSoundFX(float value)
    {
        PlayerPrefs.SetFloat(SOUND_FX_KEY, value);
        PlayerPrefs.Save();

        ApplySoundFX(value);
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
        // Used by haptics system when triggering vibration
    }

    #endregion

    #region Language

    public void SetLanguage(string languageCode)
    {
        PlayerPrefs.SetString(LANGUAGE_KEY, languageCode);
        PlayerPrefs.Save();

        ApplyLanguage(languageCode);
    }

    public string GetLanguage()
    {
        return PlayerPrefs.GetString(LANGUAGE_KEY, DEFAULT_LANGUAGE);
    }

    private void ApplyLanguage(string languageCode)
    {
        OnLanguageChanged?.Invoke(languageCode);
        // Hook into Localization system here
    }

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
