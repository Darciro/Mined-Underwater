using UnityEngine;

/// <summary>
/// ScriptableObject that defines a single objective template.
/// Create instances via Assets > Create > Mined Underwater > Objective Data.
/// </summary>
[CreateAssetMenu(fileName = "Objective_New", menuName = "Mined Underwater/Objective Data")]
public class ObjectiveData : ScriptableObject
{
    public string objectiveName;

    [Tooltip("What kind of action the player must perform")]
    public ObjectiveType type;

    [Tooltip("How many times the action must be performed")]
    [Min(1)] public int targetAmount = 1;

    [Tooltip("Only used for KillSpecificEnemy: must match the EnemyTypeName set on the enemy prefab")]
    public string enemyTypeName = "";

    /// <summary>
    /// Returns a human-readable description of this objective.
    /// </summary>
    public string GetDescription()
    {
        return objectiveName;
    }
}
