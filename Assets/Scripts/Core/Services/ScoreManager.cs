using UnityEngine;

/// <summary>
/// Manager for tracking collectibles (eggs and coins) in the current level.
/// Acts as an intermediary between pickup objects and GameManager.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    private int eggsCollected = 0;
    private int coinsCollected = 0;

    public int GetEggsCollected()
    {
        return eggsCollected;
    }

    public void AddEgg()
    {
        eggsCollected++;

        // Notify GameManager for persistent tracking and win condition check
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddEgg();
        }
    }

    public void ResetEggs()
    {
        eggsCollected = 0;
    }

    public int GetCoinsCollected()
    {
        return coinsCollected;
    }

    public void AddCoin()
    {
        coinsCollected++;

        // Notify GameManager for persistent tracking
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddCoin();
        }
    }

    public void ResetCoins()
    {
        coinsCollected = 0;
    }
}
