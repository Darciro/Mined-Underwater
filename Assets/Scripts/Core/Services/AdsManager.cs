using Unity.Services.LevelPlay;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;

    [Header("LevelPlay Settings")]
    [SerializeField] private string androidAppKey = "24cfdfd7d";
    [SerializeField] private string bannerAdUnitId = "hyu6gcwiyt8j01k7";
    [SerializeField] private string interstitialAdUnitId = "mxgkoyqso89sk0r0";

    [Header("Banner Settings")]
    [SerializeField] private bool showBannerOnInit = true;

    private bool isInitialized;
    private LevelPlayBannerAd bannerAd;
    private LevelPlayInterstitialAd interstitialAd;
    private System.Action onInterstitialClosedCallback;
    private bool showInterstitialWhenReady;

    // ------------------------
    // Unity Lifecycle
    // ------------------------
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeAds();
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnApplicationPause(bool isPaused)
    {
        // Unity Mediation handles this automatically
    }

    // ------------------------
    // Initialization
    // ------------------------
    private void InitializeAds()
    {

        try
        {
            // Register initialization callbacks
            LevelPlay.OnInitSuccess += OnSdkInitialized;
            LevelPlay.OnInitFailed += OnSdkInitializationFailed;

            // Initialize the SDK
            LevelPlay.Init(androidAppKey);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Unity LevelPlay initialization failed: {e.Message}");
        }
    }

    private void OnSdkInitialized(LevelPlayConfiguration config)
    {
        isInitialized = true;

        // Initialize ad instances
        InitializeAds_Internal();

        if (showBannerOnInit)
        {
            LoadBanner();
        }
    }

    private void OnSdkInitializationFailed(LevelPlayInitError error)
    {
        Debug.LogError($"Unity LevelPlay initialization failed: {error}");
    }

    private void InitializeAds_Internal()
    {
        // Create Banner Ad
        bannerAd = new LevelPlayBannerAd(bannerAdUnitId);

        // Register to Banner events
        bannerAd.OnAdLoaded += OnBannerLoaded;
        bannerAd.OnAdLoadFailed += OnBannerLoadFailed;
        bannerAd.OnAdDisplayed += OnBannerDisplayed;
        bannerAd.OnAdDisplayFailed += OnBannerDisplayFailed;

        // Create Interstitial Ad
        interstitialAd = new LevelPlayInterstitialAd(interstitialAdUnitId);

        // Register to Interstitial events
        interstitialAd.OnAdLoaded += OnInterstitialLoaded;
        interstitialAd.OnAdLoadFailed += OnInterstitialLoadFailed;
        interstitialAd.OnAdDisplayed += OnInterstitialDisplayed;
        interstitialAd.OnAdDisplayFailed += OnInterstitialDisplayFailed;
        interstitialAd.OnAdClosed += OnInterstitialClosed;
    }

    // ------------------------
    // Interstitial Ads
    // ------------------------
    public void LoadInterstitial()
    {
        if (!isInitialized || interstitialAd == null)
        {
            Debug.LogWarning("LevelPlay not initialized.");
            return;
        }

        try
        {
            interstitialAd.LoadAd();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Interstitial load failed: {e.Message}");
        }
    }

    public void ShowInterstitial(System.Action onClosed = null)
    {
        if (!isInitialized || interstitialAd == null)
        {
            Debug.LogWarning("LevelPlay not initialized.");
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
                Debug.LogWarning("Interstitial ad not ready. Loading and will show when ready...");
                onInterstitialClosedCallback = onClosed;
                showInterstitialWhenReady = true;
                LoadInterstitial();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Interstitial show failed: {e.Message}");
            onClosed?.Invoke();
        }
    }

    // ------------------------
    // Banner Ads
    // ------------------------
    public void LoadBanner()
    {
        if (!isInitialized || bannerAd == null)
        {
            Debug.LogWarning("LevelPlay not initialized or banner ad is null.");
            return;
        }

        try
        {
            bannerAd.LoadAd();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Banner load failed: {e.Message}");
        }
    }

    public void ShowBanner()
    {
        if (bannerAd == null) return;

        if (GameManager.Instance != null && GameManager.Instance.CurrentState == GameStateEnum.Playing)
        {
            Debug.LogWarning("Cannot show banner during Playing state.");
            return;
        }

        bannerAd.ShowAd();
    }

    public void HideBanner()
    {
        if (bannerAd != null)
        {
            bannerAd.HideAd();
        }
    }

    public void DestroyBanner()
    {
        if (bannerAd != null)
        {
            bannerAd.DestroyAd();
        }
    }

    // ------------------------
    // Event Handlers
    // ------------------------
    private void OnBannerLoaded(LevelPlayAdInfo adInfo)
    {

        // Hide banner immediately if the game is actively being played
        if (GameManager.Instance != null && (GameManager.Instance.CurrentState == GameStateEnum.Playing || GameManager.Instance.CurrentState == GameStateEnum.LevelComplete))
            HideBanner();
    }

    private void HandleGameStateChanged(GameStateEnum newState)
    {
        if (newState == GameStateEnum.Playing || newState == GameStateEnum.LevelComplete)
            HideBanner();
        else
            ShowBanner();
    }

    private void OnBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Banner load failed: {error}");
    }

    private void OnBannerDisplayed(LevelPlayAdInfo adInfo)
    {
    }

    private void OnBannerDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Banner display failed: {error}");
    }

    private void OnInterstitialLoaded(LevelPlayAdInfo adInfo)
    {

        // If ShowInterstitial was called while the ad wasn't ready, show it now
        if (showInterstitialWhenReady)
        {
            showInterstitialWhenReady = false;
            interstitialAd.ShowAd();
        }
    }

    private void OnInterstitialLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial load failed: {error}");
    }

    private void OnInterstitialDisplayed(LevelPlayAdInfo adInfo)
    {
        HideBanner();
    }

    private void OnInterstitialDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial display failed: {error}");

        // Invoke and clear the callback so callers aren't left waiting
        System.Action callback = onInterstitialClosedCallback;
        onInterstitialClosedCallback = null;
        showInterstitialWhenReady = false;
        callback?.Invoke();
    }

    private void OnInterstitialClosed(LevelPlayAdInfo adInfo)
    {

        // Restore banner
        ShowBanner();

        // Invoke and clear the callback
        System.Action callback = onInterstitialClosedCallback;
        onInterstitialClosedCallback = null;
        callback?.Invoke();

        // Auto-load next interstitial
        LoadInterstitial();
    }

    private void OnDestroy()
    {
        // Unregister callbacks
        LevelPlay.OnInitSuccess -= OnSdkInitialized;
        LevelPlay.OnInitFailed -= OnSdkInitializationFailed;

        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;

        if (bannerAd != null)
        {
            bannerAd.OnAdLoaded -= OnBannerLoaded;
            bannerAd.OnAdLoadFailed -= OnBannerLoadFailed;
            bannerAd.OnAdDisplayed -= OnBannerDisplayed;
            bannerAd.OnAdDisplayFailed -= OnBannerDisplayFailed;
        }

        if (interstitialAd != null)
        {
            interstitialAd.OnAdLoaded -= OnInterstitialLoaded;
            interstitialAd.OnAdLoadFailed -= OnInterstitialLoadFailed;
            interstitialAd.OnAdDisplayed -= OnInterstitialDisplayed;
            interstitialAd.OnAdDisplayFailed -= OnInterstitialDisplayFailed;
            interstitialAd.OnAdClosed -= OnInterstitialClosed;
        }
    }
}
