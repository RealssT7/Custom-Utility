using UnityEngine;

namespace CustomUtility.PlayUtility
{
    /// <summary>
    ///     Base class for player control, defining a method to set the player's position.
    /// </summary>
    public class PlayerCustomPlay : MonoBehaviour
    {
        /// <summary>
        ///     Custom method to set the player's position.
        /// </summary>
        /// <param name="position">The new position to place the player at.</param>
        public virtual void SetPosition(Vector3 position)
        {
        }
    }
}