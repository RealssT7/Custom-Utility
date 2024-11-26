using UnityEngine;

namespace CustomUtility.SelectionUtility
{
    [CreateAssetMenu(fileName = "SelectionUtilityEditorData", menuName = "Scriptable/CustomUtility/SelectionUtility")]
    public class SelectionUtilityEditorData : ScriptableObject
    {
        public string addButtonStyle, buttonOffStyle, buttonSelectedStyle;
    }
}