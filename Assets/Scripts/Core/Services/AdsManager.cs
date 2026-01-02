using UnityEngine;
using UnityEngine.Advertisements;

public class AdsManager : MonoBehaviour, IUnityAdsInitializationListener, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdsManager Instance;

    [Header("Unity Ads Settings")]
    [SerializeField] private string androidGameId = "6017771";
    [SerializeField] private string iosGameId = "6017770";
    [SerializeField] private bool testMode = true;

    [Header("Placement IDs")]
    [SerializeField] private string bannerPlacementId = "Banner_Android";
    [SerializeField] private string interstitialPlacementId = "Interstitial_Android";

    [Header("Banner Ads Configuration")]
    [SerializeField] private bool showBannerOnStart = true;

    private void Awake()
    {
        Instance = this;
        InitializeAds();
    }

    private void Start()
    {
        if (showBannerOnStart)
        {
            ShowBannerAds();
        }
    }

    private void InitializeAds()
    {
        string gameId = (Application.platform == RuntimePlatform.IPhonePlayer) ? iosGameId : androidGameId;

        if (Advertisement.isInitialized)
        {
            Debug.Log("Unity Ads is already initialized.");
            return;
        }

        Advertisement.Initialize(gameId, testMode, this);
    }

    public void ShowAds()
    {
        Advertisement.Load(interstitialPlacementId, this);
        Advertisement.Show(interstitialPlacementId, this);
    }

    public void ShowBannerAds()
    {
        Advertisement.Banner.SetPosition(BannerPosition.BOTTOM_LEFT);
        Advertisement.Banner.Load(bannerPlacementId);
        Advertisement.Banner.Show(bannerPlacementId);
    }

    public void OnInitializationComplete()
    {
        Debug.Log("Unity Ads initialization complete.");
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {
        Debug.Log($"Unity Ads Initialization Failed: {error.ToString()} - {message}");
    }

    public void OnUnityAdsAdLoaded(string adUnitId)
    {
        Debug.Log($"Ad Loaded: {adUnitId}");
    }

    public void OnUnityAdsFailedToLoad(string adUnitId, UnityAdsLoadError error, string message)
    {
        Debug.Log($"Error loading Ad Unit: {adUnitId} - {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowFailure(string adUnitId, UnityAdsShowError error, string message)
    {
        Debug.Log($"Error showing Ad Unit {adUnitId}: {error.ToString()} - {message}");
    }

    public void OnUnityAdsShowStart(string adUnitId)
    {
        Debug.Log($"Ad Show Start: {adUnitId}");
    }

    public void OnUnityAdsShowClick(string adUnitId)
    {
        Debug.Log($"Ad Show Click: {adUnitId}");
    }

    public void OnUnityAdsShowComplete(string adUnitId, UnityAdsShowCompletionState showCompletionState)
    {
        Debug.Log($"Ad Show Complete: {adUnitId} - {showCompletionState}");
    }
}
