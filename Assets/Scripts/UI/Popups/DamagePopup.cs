using UnityEngine;

/// <summary>
/// Displays damage numbers with Text Animator effects.
/// Uses shake and fade animations for impact feedback.
/// </summary>
public class DamagePopup : TextAnimatorPopupBase
{
    public void Setup(int damage)
    {
        // Use shake effect for damage impact
        SetupNumeric(damage, "", "shake");
    }
}
