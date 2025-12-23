using UnityEngine;

/// <summary>
/// Displays score increment feedback with Text Animator effects.
/// Uses fade animation for clean, informative feedback.
/// </summary>
public class ScorePopup : TextAnimatorPopupBase
{
    public void Setup(int scoreValue)
    {
        // Use fade effect for clean score feedback with + prefix
        SetupNumeric(scoreValue, "+", "fade");
    }
}
