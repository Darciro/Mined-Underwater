using UnityEngine;

/// <summary>
/// Displays healing numbers with Text Animator effects.
/// Uses bounce animation for positive feedback.
/// </summary>
public class HealPopup : TextAnimatorPopupBase
{
    public void Setup(int healAmount)
    {
        // Use bounce effect for healing with + prefix
        SetupNumeric(healAmount, "+", "bounce");
    }
}
