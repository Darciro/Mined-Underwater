/// <summary>
/// Scene-level relay for AdsManager. Place this on any GameObject in a scene
/// and wire UI buttons to it. Delegates all calls to the persistent AdsManager singleton,
/// so button references are never broken when the scene-level AdsManager is destroyed.
/// </summary>
public class AdsManagerRelay : UnityEngine.MonoBehaviour
{
    public void ShowInterstitial()
    {
        if (AdsManager.Instance != null)
            AdsManager.Instance.ShowInterstitial();
        else
            UnityEngine.Debug.LogWarning("AdsManagerRelay: AdsManager instance not found.");
    }

    public void ShowBanner()
    {
        if (AdsManager.Instance != null)
            AdsManager.Instance.ShowBanner();
    }

    public void HideBanner()
    {
        if (AdsManager.Instance != null)
            AdsManager.Instance.HideBanner();
    }

    public void LoadInterstitial()
    {
        if (AdsManager.Instance != null)
            AdsManager.Instance.LoadInterstitial();
    }

    public void ShowInterstitialAndRestart()
    {
        if (AdsManager.Instance != null)
            AdsManager.Instance.ShowInterstitial(() =>
            {
                if (LevelManager.Instance != null)
                    LevelManager.Instance.RestartGame();
                else
                    UnityEngine.Debug.LogWarning("AdsManagerRelay: LevelManager instance not found.");
            });
        else
            UnityEngine.Debug.LogWarning("AdsManagerRelay: AdsManager instance not found.");
    }

    public void ShowInterstitialAndClaim()
    {
        if (AdsManager.Instance != null)
            AdsManager.Instance.ShowInterstitial(() =>
            {
                if (GameManager.Instance != null)
                    GameManager.Instance.ClaimDoubleRewards();
                else
                    UnityEngine.Debug.LogWarning("AdsManagerRelay: GameManager instance not found.");
            });
        else
            UnityEngine.Debug.LogWarning("AdsManagerRelay: AdsManager instance not found.");
    }
}
