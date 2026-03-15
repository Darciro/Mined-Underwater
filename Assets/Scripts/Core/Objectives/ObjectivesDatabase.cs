using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject database that maps stage indices to their list of objectives.
/// Create a single instance and assign it to ObjectivesManager in the scene.
/// Create via Assets > Create > Mined Underwater > Objectives Database.
/// </summary>
[CreateAssetMenu(fileName = "ObjectivesDatabase", menuName = "Mined Underwater/Objectives Database")]
public class ObjectivesDatabase : ScriptableObject
{
    [System.Serializable]
    public class StageObjectivesEntry
    {
        [Tooltip("0-based stage index (0 = tutorial)")]
        public int stageIndex;

        [Tooltip("Objectives required for this stage")]
        public List<ObjectiveData> objectives = new();
    }

    [SerializeField] private List<StageObjectivesEntry> entries = new();

    /// <summary>
    /// Returns the list of objectives defined for the given stage index.
    /// Returns an empty list if no entry exists for that stage.
    /// </summary>
    public List<ObjectiveData> GetObjectivesForStage(int stageIndex)
    {
        foreach (var entry in entries)
        {
            if (entry.stageIndex == stageIndex)
                return entry.objectives;
        }
        return new List<ObjectiveData>();
    }

    /// <summary>
    /// Returns true if at least one objective is defined for the given stage.
    /// </summary>
    public bool HasObjectivesForStage(int stageIndex)
    {
        foreach (var entry in entries)
        {
            if (entry.stageIndex == stageIndex && entry.objectives != null && entry.objectives.Count > 0)
                return true;
        }
        return false;
    }
}
