using System;
using UnityEngine;

/// <summary>
/// Tracks runtime progress for a single objective during a play session.
/// Not a MonoBehaviour — created and owned by ObjectivesManager.
/// </summary>
public class ObjectiveProgress
{
    /// <summary>The static definition of this objective.</summary>
    public ObjectiveData Data { get; }

    /// <summary>How many times the required action has been performed so far.</summary>
    public int CurrentAmount { get; private set; }

    /// <summary>True when CurrentAmount has reached or exceeded the target.</summary>
    public bool IsCompleted => CurrentAmount >= Data.targetAmount;

    /// <summary>Fired every time CurrentAmount changes.</summary>
    public event Action<ObjectiveProgress> OnProgressChanged;

    public ObjectiveProgress(ObjectiveData data)
    {
        Data = data;
        CurrentAmount = 0;
    }

    /// <summary>
    /// Increments the progress counter by <paramref name="amount"/>.
    /// Does nothing if the objective is already completed.
    /// </summary>
    public void Increment(int amount = 1)
    {
        if (IsCompleted) return;
        CurrentAmount = Mathf.Min(CurrentAmount + amount, Data.targetAmount);
        OnProgressChanged?.Invoke(this);
    }
}
