/// <summary>
/// Defines the type of action required to progress an objective.
/// </summary>
public enum ObjectiveType
{
    /// <summary>Kill X enemies of any type.</summary>
    KillAnyEnemy,

    /// <summary>Kill X enemies that match a specific named enemy type.</summary>
    KillSpecificEnemy,

    /// <summary>Collect X coins during the level.</summary>
    CollectCoins
}
