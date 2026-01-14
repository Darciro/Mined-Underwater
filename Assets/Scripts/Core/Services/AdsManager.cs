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

    private void OnApplicationPause(bool isPaused)
    {
        // Unity Mediation handles this automatically
    }

    // ------------------------
    // Initialization
    // ------------------------
    private void InitializeAds()
    {
        Debug.Log($"Initializing Unity LevelPlay with App Key: {androidAppKey}");

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
        Debug.Log($"Unity LevelPlay initialized successfully with config: {config}");
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
            Debug.Log("Loading interstitial ad...");
            interstitialAd.LoadAd();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Interstitial load failed: {e.Message}");
        }
    }

    public void ShowInterstitial()
    {
        if (!isInitialized || interstitialAd == null)
        {
            Debug.LogWarning("LevelPlay not initialized.");
            return;
        }

        try
        {
            if (interstitialAd.IsAdReady())
            {
                Debug.Log("Showing interstitial ad...");
                interstitialAd.ShowAd();
            }
            else
            {
                Debug.LogWarning("Interstitial ad not ready. Loading now...");
                LoadInterstitial();
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Interstitial show failed: {e.Message}");
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
            Debug.Log("Loading banner...");
            bannerAd.LoadAd();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Banner load failed: {e.Message}");
        }
    }

    public void HideBanner()
    {
        if (bannerAd != null)
        {
            Debug.Log("Hiding banner...");
            bannerAd.HideAd();
        }
    }

    public void DestroyBanner()
    {
        if (bannerAd != null)
        {
            Debug.Log("Destroying banner...");
            bannerAd.DestroyAd();
        }
    }

    // ------------------------
    // Event Handlers
    // ------------------------
    private void OnBannerLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner loaded: {adInfo}");
    }

    private void OnBannerLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Banner load failed: {error}");
    }

    private void OnBannerDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Banner displayed: {adInfo}");
    }

    private void OnBannerDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Banner display failed: {error}");
    }

    private void OnInterstitialLoaded(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial loaded: {adInfo}");
    }

    private void OnInterstitialLoadFailed(LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial load failed: {error}");
    }

    private void OnInterstitialDisplayed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial displayed: {adInfo}");
    }

    private void OnInterstitialDisplayFailed(LevelPlayAdInfo adInfo, LevelPlayAdError error)
    {
        Debug.LogError($"Interstitial display failed: {error}");
    }

    private void OnInterstitialClosed(LevelPlayAdInfo adInfo)
    {
        Debug.Log($"Interstitial closed: {adInfo}");
        // Auto-load next interstitial
        LoadInterstitial();
    }

    private void OnDestroy()
    {
        // Unregister callbacks
        LevelPlay.OnInitSuccess -= OnSdkInitialized;
        LevelPlay.OnInitFailed -= OnSdkInitializationFailed;

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
