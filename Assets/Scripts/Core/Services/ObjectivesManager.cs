using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Singleton that manages objective progress for the current play session.
/// Persists across scene loads (DontDestroyOnLoad).
/// 
/// Setup: Place on a GameObject in the first scene alongside GameManager.
/// Assign an ObjectivesDatabase asset in the Inspector.
/// 
/// Call InitializeForStage(stageIndex) when a level begins.
/// Call ReportEnemyKilled / ReportCoinCollected from the relevant game events.
/// </summary>
public class ObjectivesManager : MonoBehaviour
{
    #region Singleton

    public static ObjectivesManager Instance { get; private set; }

    #endregion

    #region Inspector

    [Tooltip("Database asset that maps stage indices to their objectives")]
    [SerializeField] private ObjectivesDatabase database;

    #endregion

    #region State

    private readonly List<ObjectiveProgress> activeObjectives = new();

    /// <summary>Read-only view of the objectives for the current session.</summary>
    public IReadOnlyList<ObjectiveProgress> ActiveObjectives => activeObjectives;

    /// <summary>Exposes the database so UI can read objectives before a level starts.</summary>
    public ObjectivesDatabase Database => database;

    #endregion

    #region Events

    /// <summary>Fired whenever a single objective's progress changes.</summary>
    public event Action<ObjectiveProgress> OnObjectiveUpdated;

    /// <summary>Fired once when every active objective is completed.</summary>
    public event Action OnAllObjectivesCompleted;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #endregion

    #region Initialization

    /// <summary>
    /// Clears any previous objectives and loads the ones defined for
    /// <paramref name="stageIndex"/> from the database.
    /// Call this when a new level begins (e.g. from LevelManager.StartGame).
    /// </summary>
    public void InitializeForStage(int stageIndex)
    {
        // Unsubscribe from previous objectives
        foreach (var prev in activeObjectives)
            prev.OnProgressChanged -= HandleProgressChanged;

        activeObjectives.Clear();

        if (database == null)
        {
            Debug.LogWarning("[ObjectivesManager] No ObjectivesDatabase assigned — objectives skipped.");
            return;
        }

        var objectives = database.GetObjectivesForStage(stageIndex);
        foreach (var data in objectives)
        {
            if (data == null) continue;
            var progress = new ObjectiveProgress(data);
            progress.OnProgressChanged += HandleProgressChanged;
            activeObjectives.Add(progress);
        }

        Debug.Log($"[ObjectivesManager] Initialized {activeObjectives.Count} objective(s) for stage {stageIndex}.");
    }

    #endregion

    #region Reporting

    /// <summary>
    /// Report that an enemy was killed. Pass the enemy's type name so that
    /// KillSpecificEnemy objectives can match it.
    /// </summary>
    public void ReportEnemyKilled(string enemyTypeName)
    {
        foreach (var progress in activeObjectives)
        {
            if (progress.IsCompleted) continue;

            bool matches = progress.Data.type == ObjectiveType.KillAnyEnemy ||
                           (progress.Data.type == ObjectiveType.KillSpecificEnemy &&
                            string.Equals(progress.Data.enemyTypeName, enemyTypeName,
                                          StringComparison.OrdinalIgnoreCase));
            if (matches)
                progress.Increment();
        }
    }

    /// <summary>
    /// Report that the player collected one coin.
    /// </summary>
    public void ReportCoinCollected()
    {
        foreach (var progress in activeObjectives)
        {
            if (!progress.IsCompleted && progress.Data.type == ObjectiveType.CollectCoins)
                progress.Increment();
        }
    }

    #endregion

    #region Queries

    /// <summary>
    /// Returns true when there are no active objectives or all are completed.
    /// </summary>
    public bool AreAllObjectivesCompleted()
    {
        if (activeObjectives.Count == 0) return true;
        foreach (var p in activeObjectives)
        {
            if (!p.IsCompleted) return false;
        }
        return true;
    }

    #endregion

    #region Private

    private void HandleProgressChanged(ObjectiveProgress progress)
    {
        OnObjectiveUpdated?.Invoke(progress);

        if (AreAllObjectivesCompleted())
            OnAllObjectivesCompleted?.Invoke();
    }

    #endregion
}
