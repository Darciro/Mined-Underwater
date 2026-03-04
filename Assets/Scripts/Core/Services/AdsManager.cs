using System;
using Unity.Services.LevelPlay;
using UnityEngine;

[DisallowMultipleComponent]
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("LevelPlay Settings")]
    [SerializeField] private string androidAppKey = "24cfdfd7d";
    [SerializeField] private string bannerAdUnitId = "hyu6gcwiyt8j01k7";
    [SerializeField] private string interstitialAdUnitId = "mxgkoyqso89sk0r0";

    private bool isInitialized;
    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private Action onInterstitialClosedCallback;
    private bool showInterstitialWhenReady;

    #region Lifecycle

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        InitializeSdk();
    }

    private void OnDestroy()
    {
        LevelPlay.OnInitSuccess -= HandleSdkInitialized;
        LevelPlay.OnInitFailed -= HandleSdkInitFailed;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;

        UnsubscribeBannerEvents();
        UnsubscribeInterstitialEvents();
    }

    #endregion

    #region SDK Initialization

    private void InitializeSdk()
    {
        LevelPlay.OnInitSuccess += HandleSdkInitialized;
        LevelPlay.OnInitFailed += HandleSdkInitFailed;
        LevelPlay.Init(androidAppKey);
    }

    private void HandleSdkInitialized(LevelPlayConfiguration config)
    {
        isInitialized = true;
        CreateAdInstances();
        LoadBanner();
    }

    private void HandleSdkInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay SDK init failed: {error}");
    }

    private void CreateAdInstances()
    {
        bannerAd = new LevelPlayBannerAd(bannerAdUnitId);
        bannerAd.OnAdLoaded += HandleBannerLoaded;
        bannerAd.OnAdLoadFailed += HandleBannerLoadFailed;
        bannerAd.OnAdDisplayFailed += HandleBannerDisplayFailed;

        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);
        interstitialAd.OnAdLoaded += HandleInterstitialLoaded;
        interstitialAd.OnAdLoadFailed += HandleInterstitialLoadFailed;
        interstitialAd.OnAdDisplayed += HandleInterstitialDisplayed;
        interstitialAd.OnAdDisplayFailed += HandleInterstitialDisplayFailed;
        interstitialAd.OnAdClosed += HandleInterstitialClosed;
    }

    #endregion

    #region Banner

    public void LoadBanner()
    {
        if (!IsAdSystemReady(bannerAd)) return;
        TryExecuteAdAction(() => bannerAd.LoadAd(), "Banner load");
    }

    public void ShowBanner()
    {
        if (bannerAd == null) return;

        if (ShouldSuppressBanner())
        {
            Debug.LogWarning("Banner suppressed during gameplay.");
            return;
        }

        bannerAd.ShowAd();
    }

    public void HideBanner()
    {
        bannerAd?.HideAd();
    }

    public void DestroyBanner()
    {
        bannerAd?.DestroyAd();
    }

    private void HandleBannerLoaded(LevelPlayAdInfo adInfo)
    {
        if (ShouldSuppressBanner())
            HideBanner();
    }

    private void HandleBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Banner load failed: {error}");
    }

    private void HandleBannerDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Banner display failed: {error}");
    }

    private void UnsubscribeBannerEvents()
    {
        if (bannerAd == null) return;

        bannerAd.OnAdLoaded -= HandleBannerLoaded;
        bannerAd.OnAdLoadFailed -= HandleBannerLoadFailed;
        bannerAd.OnAdDisplayFailed -= HandleBannerDisplayFailed;
    }

    #endregion

    #region Interstitial

    public void LoadInterstitial()
    {
        if (!IsAdSystemReady(interstitialAd)) return;
        TryExecuteAdAction(() => interstitialAd.LoadAd(), "Interstitial load");
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (!IsAdSystemReady(interstitialAd))
        {
            onClosed?.Invoke();
            return;
        }

        try
        {
            if (interstitialAd.IsAdReady())
            {
                onInterstitialClosedCallback = onClosed;
                interstitialAd.ShowAd();
            }
            else
            {
                // Queue to show automatically once the ad finishes loading
                onInterstitialClosedCallback = onClosed;
                showInterstitialWhenReady = true;
                LoadInterstitial();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Interstitial show failed: {e.Message}");
            onClosed?.Invoke();
        }
    }

    private void HandleInterstitialLoaded(LevelPlayAdInfo adInfo)
    {
        if (!showInterstitialWhenReady) return;

        showInterstitialWhenReady = false;
        interstitialAd.ShowAd();
    }

    private void HandleInterstitialLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial load failed: {error}");
    }

    private void HandleInterstitialDisplayed(LevelPlayAdInfo adInfo)
    {
        HideBanner();
    }

    private void HandleInterstitialDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial display failed: {error}");
        InvokeAndClearInterstitialCallback();
    }

    private void HandleInterstitialClosed(LevelPlayAdInfo adInfo)
    {
        ShowBanner();
        InvokeAndClearInterstitialCallback();
        LoadInterstitial();
    }

    /// <summary>
    /// Safely invokes and clears the interstitial callback so callers are never left waiting.
    /// </summary>
    private void InvokeAndClearInterstitialCallback()
    {
        Action callback = onInterstitialClosedCallback;
        onInterstitialClosedCallback = null;
        showInterstitialWhenReady = false;
        callback?.Invoke();
    }

    private void UnsubscribeInterstitialEvents()
    {
        if (interstitialAd == null) return;

        interstitialAd.OnAdLoaded -= HandleInterstitialLoaded;
        interstitialAd.OnAdLoadFailed -= HandleInterstitialLoadFailed;
        interstitialAd.OnAdDisplayed -= HandleInterstitialDisplayed;
        interstitialAd.OnAdDisplayFailed -= HandleInterstitialDisplayFailed;
        interstitialAd.OnAdClosed -= HandleInterstitialClosed;
    }

    #endregion

    #region Game State

    private void HandleGameStateChanged(GameStateEnum newState)
    {
        if (ShouldSuppressBanner(newState))
            HideBanner();
        else
            ShowBanner();
    }

    /// <summary>
    /// Banner should be hidden during active gameplay and level-complete sequences.
    /// </summary>
    private static bool ShouldSuppressBanner(GameStateEnum? state = null)
    {
        if (GameManager.Instance == null) return false;

        GameStateEnum current = state ?? GameManager.Instance.CurrentState;
        return current is GameStateEnum.Playing or GameStateEnum.LevelComplete;
    }

    #endregion

    #region Helpers

    private bool IsAdSystemReady(object adInstance)
    {
        if (isInitialized && adInstance != null) return true;

        Debug.LogWarning("LevelPlay SDK not initialized or ad instance is null.");
        return false;
    }

    private static void TryExecuteAdAction(Action action, string operationName)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Debug.LogError($"{operationName} failed: {e.Message}");
        }
    }

    #endregion
}
