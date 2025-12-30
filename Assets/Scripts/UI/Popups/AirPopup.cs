using UnityEngine;

/// <summary>
/// Displays air restoration numbers with Text Animator effects.
/// Uses wave animation for air refill feedback.
/// </summary>
public class AirPopup : TextAnimatorPopupBase
{
    public void Setup(int airAmount)
    {
        // Use wave effect for air restoration with + prefix
        SetupNumeric(airAmount, "+", "wave");
    }
}
