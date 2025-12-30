using UnityEngine;

/// <summary>
/// Displays egg collection feedback with Text Animator effects.
/// Uses wiggle animation for playful, positive feedback.
/// </summary>
public class EggCollectionPopup : TextAnimatorPopupBase
{
    [Header("Egg Display")]
    [SerializeField] private string eggText = "+1 Egg";

    public void Setup()
    {
        // Use wiggle effect for playful egg collection feedback
        SetupText(eggText, "wiggle");
    }

    public void Setup(string customText)
    {
        SetupText(customText, "wiggle");
    }
}
