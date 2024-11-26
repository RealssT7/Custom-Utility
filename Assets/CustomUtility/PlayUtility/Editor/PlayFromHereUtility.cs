using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtility.PlayUtility.Editor
{
    /// <summary>
    ///     Adds a "Play from Here" button to the Scene View toolbar, allowing the user to
    ///     start play mode from the current camera position in the editor.
    /// </summary>
    [Overlay(typeof(SceneView), "Play from Here", true)]
    internal class PlayFromHereUtility : ToolbarOverlay
    {
        internal PlayFromHereUtility() : base(CreatePlayButton.ID)
        {
        }

        protected override Layout supportedLayouts => Layout.VerticalToolbar;

        /// <summary>
        ///     Creates the play button in the toolbar overlay.
        /// </summary>
        [EditorToolbarElement(ID, typeof(SceneView))]
        private class CreatePlayButton : EditorToolbarButton
        {
            // Constants to store the static data.
            public const string ID = "PlayFromHere/PlayButton";
            private const string PlayerTag = "Player";
            private const string PlayerObjectName = "PlayerCharacter";
            private const string PlayIconPath = "CustomUtility_Play_Icon_Play";

            private const string Tooltip = "Play from the current editor camera position.\n" +
                                           "To use this, rename player to <color=green>'PlayerCharacter'</color> or assign <color=green>'Player' Tag</color>.\n" +
                                           "Another way is to add the <color=green>'PlayerCustomPlay'</color> component to the player object.";

            /// <summary>
            ///     Initializes the toolbar overlay button.
            /// </summary>
            internal CreatePlayButton()
            {
                tooltip = Tooltip;
                icon = Resources.Load<Texture2D>(PlayIconPath);
                clicked += OnClick;
            }

            /// <summary>
            ///     Event handler for when the overlay button is clicked.
            /// </summary>
            private static void OnClick()
            {
                if (!EditorApplication.isPlaying) return;

                var playerObj = FindPlayerObject();
                if (playerObj == null)
                {
                    Debug.LogWarning(
                        $"Player object not found. Ensure it is named '{PlayerObjectName}', has the '{PlayerTag}' tag, or uses the 'PlayerCustomPlay' component.");
                    return;
                }

                SetPlayerPosition(playerObj, SceneView.lastActiveSceneView.camera.transform.position);
            }

            /// <summary>
            ///     Attempts to find the player object in the scene by component, tag, or name.
            /// </summary>
            /// <returns>The player GameObject if found, otherwise null.</returns>
            private static GameObject FindPlayerObject()
            {
                // Try to find the player by PlayerCustomPlay component first, then tag, and finally by name.
                var playerComponent = Object.FindObjectOfType<PlayerCustomPlay>();
                return playerComponent?.gameObject
                       ?? GameObject.FindWithTag(PlayerTag)
                       ?? GameObject.Find(PlayerObjectName);
            }

            /// <summary>
            ///     Sets the player object position, utilizing PlayerCustomPlay if available.
            /// </summary>
            /// <param name="playerObj">The player GameObject.</param>
            /// <param name="position">The new position to set.</param>
            private static void SetPlayerPosition(GameObject playerObj, Vector3 position)
            {
                var playerComponent = playerObj.GetComponent<PlayerCustomPlay>();
                if (playerComponent != null)
                    playerComponent.SetPosition(position);
                else
                    playerObj.transform.position = position;
            }
        }
    }
}