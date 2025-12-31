using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private int score = 0;
    private int eggsCollected = 0;

    public int GetScore()
    {
        return score;
    }

    public void ModifyScore(int scoreToAdd)
    {
        score += scoreToAdd;
        score = Mathf.Clamp(score, 0, int.MaxValue);

        // Award user points at 1:1 ratio (persistent currency)
        if (scoreToAdd > 0 && GameManager.Instance != null)
        {
            GameManager.Instance.AddPoints(scoreToAdd);
        }
    }

    public void ResetScore()
    {
        score = 0;
    }

    public int GetEggsCollected()
    {
        return eggsCollected;
    }

    public void AddEgg()
    {
        eggsCollected++;
    }

    public void ResetEggs()
    {
        eggsCollected = 0;
    }
}
