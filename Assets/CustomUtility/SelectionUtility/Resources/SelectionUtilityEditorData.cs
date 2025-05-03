using UnityEngine;

namespace CustomUtility.SelectionUtility
{
    /// <summary>
    ///     ScriptableObject to store editor data for the Selection Utility.
    /// </summary>
    [CreateAssetMenu(fileName = "SelectionUtilityEditorData", menuName = "Scriptable/CustomUtility/SelectionUtility")]
    public class SelectionUtilityEditorData : ScriptableObject
    {
        /// <summary>
        ///     Style name for the add button.
        /// </summary>
        public string addButtonStyle;

        /// <summary>
        ///     Style name for the button when it is turned off.
        /// </summary>
        public string buttonOffStyle;

        /// <summary>
        ///     Style name for the header.
        /// </summary>
        public string headerStyle;
    }
}