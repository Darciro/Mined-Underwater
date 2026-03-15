using UnityEngine;

/// <summary>
/// Abstract base for all item use-effects.
/// Create a ScriptableObject asset for each concrete effect type and assign it
/// to the item's <see cref="Item.effect"/> field in the Inspector.
/// </summary>
public abstract class ItemEffect : ScriptableObject
{
    /// <summary>Called when the owning item is used by the player.</summary>
    public abstract void Use(PlayerController player);
}
