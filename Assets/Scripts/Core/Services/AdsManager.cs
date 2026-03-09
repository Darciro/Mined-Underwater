using System;
using Unity.Services.LevelPlay;
using UnityEngine;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance { get; private set; }

    [Header("LevelPlay Settings")]
    [SerializeField] private string androidAppKey = "24cfdfd7d";
    [SerializeField] private string bannerAdUnitId = "hyu6gcwiyt8j01k7";
    [SerializeField] private string interstitialAdUnitId = "mxgkoyqso89sk0r0";

    private LevelPlayBannerAd banner;
    private LevelPlayInterstitialAd interstitial;

    private bool isInitialized;
    private bool showInterstitialWhenLoaded;
    private Action pendingInterstitialClosed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        LevelPlay.OnInitSuccess += OnSdkInitialized;
        LevelPlay.OnInitFailed += OnSdkInitFailed;
    }

    private void Start()
    {
        LevelPlay.Init(androidAppKey);

        // Subscribe here instead of OnEnable so that GameManager.Instance
        // is guaranteed to be set (all Awake calls complete before any Start).
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDisable()
    {
        LevelPlay.OnInitSuccess -= OnSdkInitialized;
        LevelPlay.OnInitFailed -= OnSdkInitFailed;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;

        UnsubscribeBannerEvents();
        UnsubscribeInterstitialEvents();
    }

    private void OnSdkInitialized(LevelPlayConfiguration config)
    {
        isInitialized = true;

        banner = new LevelPlayBannerAd(bannerAdUnitId);
        banner.OnAdLoaded += OnBannerLoaded;
        banner.OnAdLoadFailed += error => Debug.LogError($"Banner load failed: {error}");
        banner.OnAdDisplayFailed += (_, error) => Debug.LogError($"Banner display failed: {error}");

        interstitial = new LevelPlayInterstitialAd(interstitialAdUnitId);
        interstitial.OnAdLoaded += OnInterstitialLoaded;
        interstitial.OnAdLoadFailed += OnInterstitialLoadFailed;
        interstitial.OnAdDisplayed += _ => HideBanner();
        interstitial.OnAdDisplayFailed += OnInterstitialDisplayFailed;
        interstitial.OnAdClosed += OnInterstitialClosed;

        LoadBanner();
        LoadInterstitial();
    }

    private void OnSdkInitFailed(LevelPlayInitError error)
    {
        Debug.LogError($"LevelPlay SDK init failed: {error}");
    }

    public void LoadBanner()
    {
        if (!CanUseAds() || IsBannerSuppressed()) return;
        SafeCall(() => banner.LoadAd(), "Banner load");
    }

    public void ShowBanner()
    {
        if (banner == null || IsBannerSuppressed()) return;
        SafeCall(() => banner.ShowAd(), "Banner show");
    }

    public void HideBanner() => banner?.HideAd();

    public void DestroyBanner() => banner?.DestroyAd();

    public void LoadInterstitial()
    {
        if (!CanUseAds()) return;
        SafeCall(() => interstitial.LoadAd(), "Interstitial load");
    }

    public void ShowInterstitial(Action onClosed = null)
    {
        if (!CanUseAds())
        {
            onClosed?.Invoke();
            return;
        }

        pendingInterstitialClosed = onClosed;

        if (interstitial.IsAdReady())
        {
            SafeCall(() => interstitial.ShowAd(), "Interstitial show");
            return;
        }

        showInterstitialWhenLoaded = true;
        LoadInterstitial();
    }

    private void OnBannerLoaded(LevelPlayAdInfo adInfo)
    {
        if (IsBannerSuppressed())
            HideBanner();
        else
            ShowBanner();
    }

    private void OnInterstitialLoaded(LevelPlayAdInfo adInfo)
    {
        if (!showInterstitialWhenLoaded) return;

        showInterstitialWhenLoaded = false;
        SafeCall(() => interstitial.ShowAd(), "Interstitial show");
    }

    private void OnInterstitialLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial load failed: {error}");
        CompleteInterstitialFlow();
    }

    private void OnInterstitialDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial display failed: {error}");
        CompleteInterstitialFlow();
    }

    private void OnInterstitialClosed(LevelPlayAdInfo adInfo)
    {
        ShowBanner();
        CompleteInterstitialFlow();
        LoadInterstitial();
    }

    private void OnGameStateChanged(GameStateEnum state)
    {
        if (IsBannerSuppressed(state))
            HideBanner();
        else
            ShowBanner();
    }

    private void CompleteInterstitialFlow()
    {
        showInterstitialWhenLoaded = false;

        var callback = pendingInterstitialClosed;
        pendingInterstitialClosed = null;
        callback?.Invoke();
    }

    private bool CanUseAds()
    {
        if (isInitialized && banner != null && interstitial != null)
            return true;

        Debug.LogWarning("Ads are not ready yet.");
        return false;
    }

    private static bool IsBannerSuppressed(GameStateEnum? state = null)
    {
        if (GameManager.Instance == null) return false;

        var currentState = state ?? GameManager.Instance.CurrentState;

        Debug.Log($"Checking banner suppression for state: {currentState}");

        if (currentState == GameStateEnum.Paused && SceneManager.GetActiveScene().name == "Tutorial")
            return true;

        return currentState is not (
            GameStateEnum.MainMenu
            or GameStateEnum.Paused
            or GameStateEnum.GameOver
        );
    }

    private static void SafeCall(Action action, string name)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            Debug.LogError($"{name} failed: {e.Message}");
        }
    }

    private void UnsubscribeBannerEvents()
    {
        if (banner == null) return;

        banner.OnAdLoaded -= OnBannerLoaded;
    }

    private void UnsubscribeInterstitialEvents()
    {
        if (interstitial == null) return;

        interstitial.OnAdLoaded -= OnInterstitialLoaded;
        interstitial.OnAdLoadFailed -= OnInterstitialLoadFailed;
        interstitial.OnAdDisplayFailed -= OnInterstitialDisplayFailed;
        interstitial.OnAdClosed -= OnInterstitialClosed;
    }
}