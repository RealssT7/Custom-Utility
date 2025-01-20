using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CustomUtility.SelectionUtility.Editor
{
    /// <summary>
    ///     Adds a "Selection Utility" window to the Custom Utility Menu Tab, allowing the user to
    ///     select and rename GameObjects in the Unity scene based on component and naming criteria.
    /// </summary>
    public class SelectionUtility : EditorWindow
    {
        #region Enums Data

        /// <summary>
        ///     Enum representing the tool category.
        /// </summary>
        private enum ToolCategory
        {
            Select,
            Rename,
            Replace
        }

        private ToolCategory _currentToolCategory;

        /// <summary>
        ///     Enum representing the selection type.
        /// </summary>
        private enum SelectionType
        {
            Component,
            Name,
            Tag,
            Layer
        }

        private SelectionType _currentSelectionType;

        /// <summary>
        ///     Enum representing the logic mode.
        /// </summary>
        private enum LogicMode
        {
            Auto,
            Manual
        }

        private LogicMode _currentLogicMode;

        /// <summary>
        ///     Enum representing the selection name type for filtering.
        /// </summary>
        private enum SelectionNameType
        {
            Contains,
            ContainsExactly,
            DoNotContain,
            Equal,
            DoNotEqual,
            StartWith,
            DoNotStartWith,
            EndWith,
            DoNotEndWith
        }

        private SelectionNameType _currentSelectionNameType;

        /// <summary>
        ///     Enum representing the renaming type.
        /// </summary>
        private enum RenamingType
        {
            ReplaceCompletely,
            ReplaceInName,
            AddStart,
            AddEnd,
            RemoveStart,
            RemoveEnd
        }

        private RenamingType _currentRenamingType;

        #endregion

        #region Data

        private const float FieldSpace = 20f; // Space between fields in the UI.
        private const float IndentSpace = 2.5f; // Indentation for nested UI elements.
        private const float PropertySpace = 75f; // Space for property fields in the UI.
        private const float ButtonHeight = 50f; // Height of buttons in the UI.
        private static readonly Vector2 MinSize = new(500, 500); // Minimum window size.
        private static readonly Vector2 MaxSize = new(500, 1080); // Maximum window size.

        [SerializeField] private List<GameObject> parents; // List of parent GameObjects for manual selection.
        [SerializeField] private List<GameObject> selections; // List of resulting GameObjects after filtering.

        [SerializeField] private GameObject replacementObject;
        [SerializeField] private bool keepPosition = true; // Flag to keep the original position when replacing.
        [SerializeField] private bool keepRotation = true; // Flag to keep the original rotation when replacing.
        [SerializeField] private bool keepScale = true; // Flag to keep the original scale when replacing.
        [SerializeField] private bool keepParent = true; // Flag to keep the original parent when replacing.
        [SerializeField] private bool keepName; // Flag to keep the original name when replacing.
        [SerializeField] private bool keepTag; // Flag to keep the original tags when replacing.
        [SerializeField] private bool keepLayer; // Flag to keep the original layer when replacing.
        [SerializeField] private bool keepComponent; // Flag to keep the original components when replacing.

        private string[] _cachedComponent, _cachedTag; // Cached component type & tag names.
        private SelectionUtilityEditorData _editorData; // Custom editor skin and data.
        private int _removeCharacterCount = 1; // Number of characters to remove during renaming.
        private string _renameTextValue, _renameTextValue2; // Text fields for rename operations.
        private ScriptableObject _scriptableObj; // Reference to the scriptable object managing the UI state.
        private string _searchKeyword = ""; // Keyword for searching components.
        private int _selectPopUpIndex; // Selected index from the dropdown.
        private string _selectionNameValue; // Name value used in filtering.
        private SerializedObject _serialObj; // Serialized object for managing data.
        private GUISkin _skin; // Custom skin for the UI.
        private Vector2 _viewScrollPosition; // Scroll position for the main window.

        /// <summary>
        ///     Initializes resources when the window is opened or reloaded.
        /// </summary>
        private void OnEnable()
        {
            _scriptableObj = this;
            _serialObj = new SerializedObject(_scriptableObj);
            _skin = Resources.Load<GUISkin>("SelectionUtilitiesEditorSkin");
            _editorData = Resources.Load<SelectionUtilityEditorData>("SelectionUtilitiesEditorData");
            _cachedComponent = GetAvailableComponentTypes().Select(type => type.Name).ToArray();
            _cachedTag = InternalEditorUtility.tags;

            Selection.selectionChanged += OnSelectionChanged;
            OnSelectionChanged(); // Force update the selection list
        }

        /// <summary>
        ///     Cleans up resources when the window is closed.
        /// </summary>
        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }

        #endregion

        #region EditorWindow

        /// <summary>
        ///     Opens the Selection Utility window from the Unity menu.
        /// </summary>
        [MenuItem("Custom Utility/Selection Utility")]
        private static void OpenWindow()
        {
            var window = GetWindow(typeof(SelectionUtility));
            window.minSize = MinSize;
            window.maxSize = MaxSize;
            window.Show();
        }

        /// <summary>
        ///     Renders the editor window UI.
        /// </summary>
        private void OnGUI()
        {
            if (_skin == null) return;

            _viewScrollPosition = EditorGUILayout.BeginScrollView(_viewScrollPosition, false, false);

            EditorGUILayout.BeginHorizontal();

            float width = Screen.width;

            EditorGUILayout.LabelField("Selection Utility", _skin.GetStyle(_editorData.headerStyle),
                GUILayout.MaxWidth(width), GUILayout.Height(PropertySpace));
            EditorGUILayout.EndHorizontal();

            DrawUtilities();
            _serialObj.ApplyModifiedProperties();

            GUILayout.Space(FieldSpace);
            GUILayout.EndScrollView();
        }

        /// <summary>
        ///     Draws the main utility UI elements.
        /// </summary>
        private void DrawUtilities()
        {
            GUILayout.Space(FieldSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.LabelField("Action:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _currentToolCategory =
                (ToolCategory)EditorGUILayout.EnumPopup(_currentToolCategory, GUILayout.ExpandWidth(true));

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(IndentSpace);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            switch (_currentToolCategory)
            {
                case ToolCategory.Select:
                    DrawSelectingTools();
                    break;
                case ToolCategory.Rename:
                    DrawRenamingTools();
                    break;
                case ToolCategory.Replace:
                    DrawReplacingTools();
                    break;
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawSelectedObjectList();

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        ///     Draws the list of selecting GameObjects.
        /// </summary>
        private void DrawSelectedObjectList()
        {
            EditorGUILayout.LabelField("Selection:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            var serialProp = _serialObj.FindProperty("selections");
            EditorGUILayout.PropertyField(serialProp, true);
        }

        /// <summary>
        ///     Handles the selection change event.
        /// </summary>
        private void OnSelectionChanged()
        {
            selections = Selection.objects.Select(obj => (GameObject)obj).ToList();
            if (_serialObj != null && _serialObj.targetObject != null) _serialObj.Update();
        }

        #endregion

        #region Selection Tool

        private void DrawSelectingTools()
        {
            EditorGUILayout.LabelField("Select By:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _currentSelectionType = (SelectionType)EditorGUILayout.EnumPopup(_currentSelectionType);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(IndentSpace);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            switch (_currentSelectionType)
            {
                case SelectionType.Component:
                    DrawComponentSelection();
                    break;
                case SelectionType.Name:
                    DrawNameSelection();
                    break;
                case SelectionType.Tag:
                    DrawTagSelection();
                    break;
                case SelectionType.Layer:
                    DrawLayerSelection();
                    break;
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(IndentSpace);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawSearchModeSelection();

            if (_currentLogicMode == LogicMode.Manual)
            {
                GUILayout.Space(FieldSpace);
                EditorGUILayout.EndHorizontal();

                GUILayout.Space(IndentSpace);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(FieldSpace);

                DrawManualSearchMode();
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawSelectButton();
        }

        private void DrawComponentSelection()
        {
            EditorGUILayout.LabelField("Component:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            var typeNames = _cachedComponent;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _searchKeyword = EditorGUILayout.TextField(_searchKeyword, GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(_searchKeyword) && GUILayout.Button("\u2715", GUILayout.Width(FieldSpace)))
            {
                _searchKeyword = "";
                GUI.FocusControl("");
            }

            if (EditorGUI.EndChangeCheck())
            {
                _selectPopUpIndex = 0;
                Repaint();
            }

            GUILayout.Space(IndentSpace * 2);
            var filteredTypeNames = FilterTypeNames(typeNames);

            if (filteredTypeNames.Length == 0)
                EditorGUILayout.LabelField("No components found.", GUILayout.ExpandWidth(true));
            else
                _selectPopUpIndex =
                    EditorGUILayout.Popup(_selectPopUpIndex,
                        filteredTypeNames); // The popup list of the components needs to be improved.

            EditorGUILayout.EndHorizontal();
        }

        private void DrawNameSelection()
        {
            EditorGUILayout.LabelField("Text Value:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _selectionNameValue = EditorGUILayout.TextField(_selectionNameValue, GUILayout.ExpandWidth(true));

            GUILayout.Space(IndentSpace * 2);

            EditorGUILayout.LabelField("Name Rule:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _currentSelectionNameType = (SelectionNameType)EditorGUILayout.EnumPopup(_currentSelectionNameType);
        }

        private void DrawTagSelection()
        {
            EditorGUILayout.LabelField("Tag:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _selectPopUpIndex = EditorGUILayout.Popup(_selectPopUpIndex, _cachedTag, GUILayout.ExpandWidth(true));
        }

        private void DrawLayerSelection()
        {
            EditorGUILayout.LabelField("Layer:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _selectPopUpIndex = EditorGUILayout.LayerField(_selectPopUpIndex, GUILayout.ExpandWidth(true));
        }

        private void DrawSearchModeSelection()
        {
            EditorGUILayout.LabelField("Mode:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _currentLogicMode = (LogicMode)EditorGUILayout.EnumPopup(_currentLogicMode);
        }

        private void DrawManualSearchMode()
        {
            EditorGUILayout.LabelField("Select In:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            var serialProp = _serialObj.FindProperty("parents");
            EditorGUILayout.PropertyField(serialProp, true);
        }

        private void DrawSelectButton()
        {
            if (GUILayout.Button("OBJECT SELECTION", _skin.GetStyle(_editorData.addButtonStyle),
                    GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(true)))
            {
                Selection.objects = null;
                selections = new List<GameObject>();
                var componentType = GetComponentTypeFromDropdown();

                if (_currentLogicMode == LogicMode.Manual)
                {
                    if (parents.Count == 0)
                    {
                        Debug.LogWarning("No parent objects selected.");
                        return;
                    }

                    switch (_currentSelectionType)
                    {
                        case SelectionType.Component:
                            foreach (var go in parents)
                                selections.AddRange(go.GetComponentsInChildren(componentType)
                                    .Select(component => component.gameObject));
                            break;
                        case SelectionType.Name:
                            foreach (var go in parents)
                                selections.AddRange(go.GetComponentsInChildren<Transform>()
                                    .Where(transform => IsMatchingNameRules(transform.name))
                                    .Select(transform => transform.gameObject));
                            break;
                        case SelectionType.Tag:
                            foreach (var go in parents)
                                selections.AddRange(go.GetComponentsInChildren<GameObject>()
                                    .Where(child => child.CompareTag(_cachedTag[_selectPopUpIndex])));
                            break;
                        case SelectionType.Layer:
                            foreach (var go in parents)
                                selections.AddRange(go.GetComponentsInChildren<GameObject>()
                                    .Where(child => child.layer == _selectPopUpIndex));
                            break;
                    }
                }
                else
                {
                    switch (_currentSelectionType)
                    {
                        case SelectionType.Component:
                            selections.AddRange(FindObjectsOfType(componentType)
                                .Select(component => ((Component)component).gameObject));
                            break;
                        case SelectionType.Name:
                            selections.AddRange(FindObjectsOfType<Transform>()
                                .Where(transform => IsMatchingNameRules(transform.name))
                                .Select(transform => transform.gameObject));
                            break;
                        case SelectionType.Tag:
                            selections.AddRange(FindObjectsOfType<GameObject>()
                                .Where(go => go.CompareTag(_cachedTag[_selectPopUpIndex])));
                            break;
                        case SelectionType.Layer:
                            selections.AddRange(FindObjectsOfType<GameObject>()
                                .Where(go => go.layer == _selectPopUpIndex));
                            break;
                    }
                }

                Selection.objects = selections.ToArray<Object>();
                _serialObj.Update();
            }
        }

        #endregion

        #region RenamingTool

        private void DrawRenamingTools()
        {
            EditorGUILayout.LabelField("Type:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            _currentRenamingType = (RenamingType)EditorGUILayout.EnumPopup(_currentRenamingType);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(IndentSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            switch (_currentRenamingType)
            {
                case RenamingType.ReplaceCompletely:
                    EditorGUILayout.LabelField("Replace by:", GUILayout.ExpandWidth(false),
                        GUILayout.MaxWidth(PropertySpace));
                    _renameTextValue = EditorGUILayout.TextField(_renameTextValue, GUILayout.ExpandWidth(true));
                    break;
                case RenamingType.ReplaceInName:
                    EditorGUILayout.LabelField("Replace:", GUILayout.ExpandWidth(false),
                        GUILayout.MaxWidth(PropertySpace));
                    _renameTextValue = EditorGUILayout.TextField(_renameTextValue, GUILayout.ExpandWidth(true));

                    GUILayout.Space(FieldSpace);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(IndentSpace);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(FieldSpace);
                    EditorGUILayout.LabelField("By:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));

                    _renameTextValue2 = EditorGUILayout.TextField(_renameTextValue2, GUILayout.ExpandWidth(true));
                    break;
                case RenamingType.AddStart:
                case RenamingType.AddEnd:
                    EditorGUILayout.LabelField("Add:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
                    _renameTextValue = EditorGUILayout.TextField(_renameTextValue, GUILayout.ExpandWidth(true));
                    break;
                case RenamingType.RemoveStart:
                case RenamingType.RemoveEnd:
                    EditorGUILayout.LabelField("Count:", GUILayout.ExpandWidth(false),
                        GUILayout.MaxWidth(PropertySpace));
                    _removeCharacterCount =
                        EditorGUILayout.IntField(_removeCharacterCount, GUILayout.ExpandWidth(true));
                    break;
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(IndentSpace);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawRenameButton();
        }

        private void DrawRenameButton()
        {
            if (GUILayout.Button("RENAME SELECTION", _skin.GetStyle(_editorData.addButtonStyle),
                    GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(true)))
            {
                if (Selection.objects.Length == 0)
                {
                    Debug.LogWarning("No objects selected.");
                    return;
                }

                foreach (var selectedObject in Selection.objects)
                {
                    Undo.RecordObject(selectedObject, selectedObject.name);
                    selectedObject.name = GetNewName(selectedObject.name);
                }
            }
        }

        /// <summary>
        ///     Gets the new name for a GameObject based on the renaming type.
        /// </summary>
        /// <param name="objectName">The original name of the GameObject.</param>
        /// <returns>The new name of the GameObject.</returns>
        private string GetNewName(string objectName)
        {
            switch (_currentRenamingType)
            {
                case RenamingType.ReplaceCompletely:
                    return _renameTextValue;
                case RenamingType.ReplaceInName:
                    return objectName.Replace(_renameTextValue, _renameTextValue2);
                case RenamingType.AddStart:
                    return _renameTextValue + objectName;
                case RenamingType.AddEnd:
                    return objectName + _renameTextValue;
                case RenamingType.RemoveStart:
                    return objectName.Length < _removeCharacterCount
                        ? objectName
                        : objectName.Remove(0, _removeCharacterCount);
                case RenamingType.RemoveEnd:
                    return objectName.Length < _removeCharacterCount
                        ? objectName
                        : objectName.Remove(objectName.Length - _removeCharacterCount, _removeCharacterCount);
            }

            return objectName;
        }

        #endregion

        #region ReplaceTool

        private void DrawReplacingTools()
        {
            EditorGUILayout.LabelField("Replace By:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));
            replacementObject = (GameObject)EditorGUILayout.ObjectField(replacementObject, typeof(GameObject), true,
                GUILayout.ExpandWidth(true));

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(FieldSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.LabelField("Keep Rule:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(PropertySpace));

            EditorGUILayout.BeginVertical();
            var defaultWidth = EditorGUIUtility.labelWidth;
            EditorGUIUtility.labelWidth = PropertySpace;

            keepPosition = EditorGUILayout.Toggle("Position", keepPosition);
            keepRotation = EditorGUILayout.Toggle("Rotation", keepRotation);
            keepScale = EditorGUILayout.Toggle("Scale", keepScale);
            keepParent = EditorGUILayout.Toggle("Parent", keepParent);
            keepName = EditorGUILayout.Toggle("Name", keepName);
            keepTag = EditorGUILayout.Toggle("Tag", keepTag);
            keepLayer = EditorGUILayout.Toggle("Layer", keepLayer);
            keepComponent = EditorGUILayout.Toggle("Component", keepComponent);

            EditorGUIUtility.labelWidth = defaultWidth;
            EditorGUILayout.EndVertical();

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(FieldSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawReplaceButton();
        }

        private void DrawReplaceButton()
        {
            if (GUILayout.Button("REPLACE SELECTION", _skin.GetStyle(_editorData.addButtonStyle),
                    GUILayout.Height(ButtonHeight), GUILayout.ExpandWidth(true)))
            {
                if (replacementObject == null)
                {
                    Debug.LogWarning("No replacement prefab selected.");
                    return;
                }

                if (selections == null || selections.Count == 0)
                {
                    Debug.LogWarning("No objects in results to replace.");
                    return;
                }

                ReplaceSelectedObjects();
            }
        }

        /// <summary>
        ///     Replaces the selected GameObjects with a new GameObject.
        /// </summary>
        private void ReplaceSelectedObjects()
        {
            Undo.IncrementCurrentGroup();
            var undoGroup = Undo.GetCurrentGroup();
            var newSelections = new List<Object>();

            foreach (var target in selections)
            {
                Undo.RecordObject(target, "Replace Object");
                GameObject replacement;

                if (PrefabUtility.IsPartOfPrefabAsset(replacementObject) ||
                    PrefabUtility.IsPartOfPrefabInstance(replacementObject))
                    replacement = (GameObject)PrefabUtility.InstantiatePrefab(replacementObject);
                else replacement = Instantiate(replacementObject);

                Undo.RegisterCreatedObjectUndo(replacement, "Create Replacement");

                if (keepParent) replacement.transform.SetParent(target.transform.parent, false);
                if (keepPosition) replacement.transform.position = target.transform.position;
                if (keepRotation) replacement.transform.rotation = target.transform.rotation;
                if (keepScale) replacement.transform.localScale = target.transform.localScale;
                if (keepName) replacement.name = target.name;
                if (keepTag) replacement.tag = target.tag;
                if (keepLayer) replacement.layer = target.layer;

                if (keepComponent)
                    foreach (var component in target.GetComponents<Component>())
                    {
                        var existingComponent = replacement.GetComponent(component.GetType());

                        if (existingComponent != null)
                        {
                            Undo.RecordObject(existingComponent, "Copy Component Data");
                            EditorUtility.CopySerialized(component, existingComponent);
                        }
                        else
                        {
                            var newComponent = replacement.AddComponent(component.GetType());
                            Undo.RegisterCreatedObjectUndo(newComponent, "Add Component");
                            EditorUtility.CopySerialized(component, newComponent);
                        }
                    }

                Undo.DestroyObjectImmediate(target);
                newSelections.Add(replacement);
            }

            Selection.objects = newSelections.ToArray();
            Undo.CollapseUndoOperations(undoGroup);
        }

        #endregion

        #region Functions

        /// <summary>
        ///     Filters the component type names based on the search keyword.
        /// </summary>
        /// <param name="typeNames">Array of component type names.</param>
        /// <returns>Filtered array of component type names.</returns>
        private string[] FilterTypeNames(string[] typeNames)
        {
            if (string.IsNullOrEmpty(_searchKeyword)) return typeNames;
            var filteredTypeNames = typeNames.Where(typeName => typeName.ToLower().Contains(_searchKeyword.ToLower()))
                .ToList();

            if (_selectPopUpIndex >= filteredTypeNames.Count) _selectPopUpIndex = 0;

            return filteredTypeNames.ToArray();
        }

        /// <summary>
        ///     Checks if a name matches the selection name rules.
        /// </summary>
        /// <param name="entryName">The name to check.</param>
        /// <returns>True if the name matches the rules, otherwise false.</returns>
        private bool IsMatchingNameRules(string entryName)
        {
            switch (_currentSelectionNameType)
            {
                case SelectionNameType.Contains:
                    return entryName.ToLower().Contains(_selectionNameValue);
                case SelectionNameType.ContainsExactly:
                    return entryName.Contains(_selectionNameValue);
                case SelectionNameType.DoNotContain:
                    return !entryName.Contains(_selectionNameValue);
                case SelectionNameType.Equal:
                    return string.Equals(entryName, _selectionNameValue);
                case SelectionNameType.DoNotEqual:
                    return !string.Equals(entryName, _selectionNameValue);
                case SelectionNameType.StartWith:
                    return entryName.StartsWith(_selectionNameValue);
                case SelectionNameType.DoNotStartWith:
                    return !entryName.StartsWith(_selectionNameValue);
                case SelectionNameType.EndWith:
                    return entryName.EndsWith(_selectionNameValue);
                case SelectionNameType.DoNotEndWith:
                    return !entryName.EndsWith(_selectionNameValue);
            }

            return false;
        }

        /// <summary>
        ///     Gets the component type from the dropdown selection.
        /// </summary>
        /// <returns>The selected component type.</returns>
        private Type GetComponentTypeFromDropdown()
        {
            var componentTypes = GetAvailableComponentTypes();

            if (_selectPopUpIndex >= 0 && _selectPopUpIndex < componentTypes.Length)
                return componentTypes[_selectPopUpIndex];
            return null;
        }

        /// <summary>
        ///     Gets the available component types in the current domain.
        /// </summary>
        /// <returns>Array of available component types.</returns>
        private Type[] GetAvailableComponentTypes()
        {
            var componentTypes = new List<Type>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                var typesInAssembly = assembly.GetTypes();
                componentTypes.AddRange(typesInAssembly.Where(type =>
                    type.IsSubclassOf(typeof(Component)) && type.Name.ToLower().Contains(_searchKeyword.ToLower())));
            }

            return componentTypes.ToArray();
        }

        #endregion
    }
}