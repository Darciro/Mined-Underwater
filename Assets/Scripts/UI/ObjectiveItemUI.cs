using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI component that displays a single objective row.
/// 
/// Attach to a prefab that has (at minimum) a description TextMeshProUGUI.
/// Wire up progressText and completedIcon in the Inspector for richer display.
/// 
/// Use Initialize(ObjectiveProgress) for live in-game tracking,
/// or InitializeStatic(ObjectiveData) for the read-only preview in the StagePopup.
/// </summary>
public class ObjectiveItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI objectiveText;
    [SerializeField] private TextMeshProUGUI objectiveAmountText;
    [SerializeField] private TextMeshProUGUI progressText;

    [Tooltip("Optional icon or checkmark shown when the objective is complete")]
    [SerializeField] private GameObject completedIcon;

    private ObjectiveProgress trackedProgress;

    // -----------------------------------------------------------------------
    // Public API
    // -----------------------------------------------------------------------

    /// <summary>
    /// Binds this item to a live <see cref="ObjectiveProgress"/> so it
    /// updates automatically as the player makes progress.
    /// </summary>
    public void Initialize(ObjectiveProgress progress)
    {
        Unbind();

        trackedProgress = progress;
        trackedProgress.OnProgressChanged += Refresh;

        Refresh(trackedProgress);
    }

    /// <summary>
    /// Shows a static snapshot of an objective (e.g. in the pre-game StagePopup)
    /// without binding to any live progress. Progress is shown as 0 / target.
    /// </summary>
    public void InitializeStatic(ObjectiveData data)
    {
        Unbind();

        if (objectiveText != null)
            objectiveText.text = data.GetDescription();

        if (objectiveAmountText != null)
            objectiveAmountText.text = $"X {data.targetAmount}";

        if (progressText != null)
            progressText.text = $"0 / {data.targetAmount}";

        if (completedIcon != null)
            completedIcon.SetActive(false);
    }

    // -----------------------------------------------------------------------
    // Private
    // -----------------------------------------------------------------------

    private void Refresh(ObjectiveProgress progress)
    {
        if (objectiveText != null)
            objectiveText.text = progress.Data.GetDescription();

        if (objectiveAmountText != null)
            objectiveAmountText.text = $"{progress.CurrentAmount} / {progress.Data.targetAmount}";

        if (progressText != null)
            progressText.text = $"{progress.CurrentAmount} / {progress.Data.targetAmount}";

        if (completedIcon != null)
            completedIcon.SetActive(progress.IsCompleted);
    }

    private void Unbind()
    {
        if (trackedProgress != null)
        {
            trackedProgress.OnProgressChanged -= Refresh;
            trackedProgress = null;
        }
    }

    private void OnDestroy() => Unbind();
}
