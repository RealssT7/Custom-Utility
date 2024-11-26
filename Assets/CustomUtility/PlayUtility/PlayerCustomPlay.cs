using UnityEngine;

/// <summary>
///     Abstract base class for player control, defining a method to set the player's position.
/// </summary>
public abstract class PlayerCustomPlay : MonoBehaviour
{
    /// <summary>
    ///     Custom method to set the player's position.
    /// </summary>
    /// <param name="position">The new position to place the player at.</param>
    public abstract void SetPosition(Vector3 position);
}