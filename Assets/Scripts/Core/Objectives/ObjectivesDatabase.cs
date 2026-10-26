using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ScriptableObject database that maps stage indices to their list of objectives
/// and the item rewards that can drop when the stage is completed.
/// Create a single instance and assign it to ObjectivesManager in the scene.
/// Create via Assets > Create > Mined Underwater > Objectives Database.
/// </summary>
[CreateAssetMenu(fileName = "ObjectivesDatabase", menuName = "Mined Underwater/Objectives Database")]
public class ObjectivesDatabase : ScriptableObject
{
    [System.Serializable]
    public class StageReward
    {
        [Tooltip("What this reward grants: an inventory item, coins or eggs")]
        public RewardType type = RewardType.Item;

        [Tooltip("Only used for Item rewards: the item granted when this reward drops")]
        public Item item;

        [Tooltip("How many items, coins or eggs are granted")]
        [Min(1)] public int amount = 1;

        [Tooltip("Chance (0-100%) of this reward dropping when the stage is completed")]
        [Range(0f, 100f)] public float dropChance = 100f;

        /// <summary>False for Item rewards with no item assigned.</summary>
        public bool IsValid => type != RewardType.Item || item != null;
    }

    [System.Serializable]
    public class StageObjectivesEntry
    {
        [Tooltip("0-based stage index (0 = tutorial)")]
        public int stageIndex;

        [Tooltip("Objectives required for this stage")]
        public List<ObjectiveData> objectives = new();

        [Tooltip("Rewards (items, coins or eggs) that may be granted on completing this stage. Each one rolls its chance independently.")]
        public List<StageReward> rewards = new();
    }

    [SerializeField] private List<StageObjectivesEntry> entries = new();

    /// <summary>
    /// Returns the list of objectives defined for the given stage index.
    /// Returns an empty list if no entry exists for that stage.
    /// </summary>
    public List<ObjectiveData> GetObjectivesForStage(int stageIndex)
    {
        var entry = FindEntry(stageIndex);
        return entry != null ? entry.objectives : new List<ObjectiveData>();
    }

    /// <summary>
    /// Returns true if at least one objective is defined for the given stage.
    /// </summary>
    public bool HasObjectivesForStage(int stageIndex)
    {
        var entry = FindEntry(stageIndex);
        return entry != null && entry.objectives != null && entry.objectives.Count > 0;
    }

    /// <summary>
    /// Returns the reward table defined for the given stage index.
    /// Returns an empty list if no entry exists for that stage.
    /// </summary>
    public List<StageReward> GetRewardsForStage(int stageIndex)
    {
        var entry = FindEntry(stageIndex);
        return entry != null && entry.rewards != null ? entry.rewards : new List<StageReward>();
    }

    /// <summary>
    /// Rolls every reward of the given stage against its drop chance and
    /// returns the rewards that dropped. May return an empty list.
    /// </summary>
    public List<StageReward> RollRewardsForStage(int stageIndex)
    {
        var dropped = new List<StageReward>();
        foreach (var reward in GetRewardsForStage(stageIndex))
        {
            if (reward == null || !reward.IsValid) continue;
            // Random.value can return exactly 1, so 100% is checked explicitly
            if (reward.dropChance >= 100f || Random.value * 100f < reward.dropChance)
                dropped.Add(reward);
        }
        return dropped;
    }

    private StageObjectivesEntry FindEntry(int stageIndex)
    {
        foreach (var entry in entries)
        {
            if (entry.stageIndex == stageIndex)
                return entry;
        }
        return null;
    }
}
