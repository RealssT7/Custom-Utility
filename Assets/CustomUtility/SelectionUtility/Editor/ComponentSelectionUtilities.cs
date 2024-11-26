using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Selection;
using Object = UnityEngine.Object;

namespace CustomUtility.SelectionUtility.Editor
{
    public class SelectionUtilityWindow : EditorWindow
    {
        private const float FieldSpace = 20f;
        private static readonly Vector2 MinSize = new(500, 500);
        private static readonly Vector2 MaxSize = new(500, 1080);

        [SerializeField] private List<GameObject> parents;
        [SerializeField] private List<GameObject> results;

        private string[] _cacheName;
        private SelectionUtilityEditorData _editorData;
        private string _numberTextPrefix, _numberTextSuffix;
        private int _removeCharacterCount = 1;
        private string _renameTextValue, _renameTextValue2;
        private ScriptableObject _scriptableObj;
        private string _searchKeyword = "";
        private int _selectedComponentIndex;
        private string _selectionNameValue;
        private SerializedObject _serialObj;
        private GUISkin _skin;
        private Vector2 _viewScrollPosition;

        private void Update()
        {
            Repaint();
        }

        private void OnEnable()
        {
            _scriptableObj = this;
            _serialObj = new SerializedObject(_scriptableObj);
            _skin = Resources.Load<GUISkin>("SelectionUtilitiesEditorSkin");
            _editorData = Resources.Load<SelectionUtilityEditorData>("SelectionUtilitiesEditorData");
            _cacheName = GetComponentTypeNames();
        }

        private void OnGUI()
        {
            if (_skin == null) return;

            DrawMainWindow();
        }

        [MenuItem("Custom Utilities/Component Selection")]
        private static void OpenWindow()
        {
            var window = GetWindow(typeof(SelectionUtilityWindow));
            window.minSize = MinSize;
            window.maxSize = MaxSize;
            window.Show();
        }

        private void DrawMainWindow()
        {
            _viewScrollPosition = EditorGUILayout.BeginScrollView(_viewScrollPosition, false, false);

            EditorGUILayout.BeginHorizontal();

            float width = Screen.width;

            EditorGUILayout.LabelField("Component Selection Utility", _skin.GetStyle(_editorData.buttonSelectedStyle),
                GUILayout.MaxWidth(width), GUILayout.Height(75));
            EditorGUILayout.EndHorizontal();

            DrawUtilities();
            _serialObj.ApplyModifiedProperties();

            GUILayout.Space(FieldSpace);
            GUILayout.EndScrollView();
        }

        private void DrawUtilities()
        {
            GUILayout.Space(FieldSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.LabelField("Action:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            _currentToolCategory =
                (ToolCategory)EditorGUILayout.EnumPopup(_currentToolCategory, GUILayout.ExpandWidth(true));

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);

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
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(FieldSpace);

            DrawResultObjectList();
        }

        private void DrawSelectingTools()
        {
            EditorGUILayout.LabelField("Select By:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            _currentSelectionType = (SelectionType)EditorGUILayout.EnumPopup(_currentSelectionType);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);

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
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawSearchModeSelection();

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            DrawSelectButton();
        }

        private void DrawComponentSelection()
        {
            EditorGUILayout.LabelField("Component:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            DrawComponentTypeDropdown();
        }

        private void DrawNameSelection()
        {
            EditorGUILayout.LabelField("Name Rule:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            _currentSelectionNameType = (SelectionNameType)EditorGUILayout.EnumPopup(_currentSelectionNameType);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.LabelField("Text Value:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            _selectionNameValue = EditorGUILayout.TextField(_selectionNameValue, GUILayout.ExpandWidth(true));
        }

        private void DrawSearchModeSelection()
        {
            EditorGUILayout.LabelField("Mode:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            _currentSearchMode = (SearchMode)EditorGUILayout.EnumPopup(_currentSearchMode);

            if (_currentSearchMode != SearchMode.Manual) return;

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.LabelField("Select In:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            var serialProp = _serialObj.FindProperty("parents");
            EditorGUILayout.PropertyField(serialProp, true);
        }

        private void DrawSelectButton()
        {
            if (GUILayout.Button("SELECT", _skin.GetStyle(_editorData.addButtonStyle), GUILayout.Height(50),
                    GUILayout.ExpandWidth(true)))
            {
                if (_currentSearchMode == SearchMode.Manual && parents.Count == 0)
                {
                    Debug.LogWarning("No parent objects selected.");
                    return;
                }

                objects = null;
                results = new List<GameObject>();

                if (_currentSelectionType == SelectionType.Component)
                {
                    if (_currentSearchMode == SearchMode.Auto)
                    {
                        var componentType = GetComponentTypeFromDropdown();
                        results.AddRange(FindObjectsOfType(componentType)
                            .Select(component => ((Component)component).gameObject));
                    }
                    else
                    {
                        foreach (var go in parents)
                        {
                            var componentType = GetComponentTypeFromDropdown();
                            results.AddRange(go.GetComponentsInChildren(componentType)
                                .Select(component => component.gameObject));
                        }
                    }
                }
                else
                {
                    if (_currentSearchMode == SearchMode.Auto)
                        results.AddRange(FindObjectsOfType<Transform>()
                            .Where(transform => IsMatchingNameRules(transform.name))
                            .Select(transform => transform.gameObject));
                    else
                        foreach (var go in parents)
                            results.AddRange(go.GetComponentsInChildren<Transform>()
                                .Where(transform => IsMatchingNameRules(transform.name))
                                .Select(transform => transform.gameObject));
                }

                objects = results.ToArray<Object>();
                _serialObj.Update();
            }
        }

        private void DrawResultObjectList()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            EditorGUILayout.LabelField("Results:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            var serialProp = _serialObj.FindProperty("results");
            EditorGUILayout.PropertyField(serialProp, true);
            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);
        }

        private void DrawComponentTypeDropdown()
        {
            var typeNames = _cacheName;

            EditorGUILayout.BeginHorizontal();
            EditorGUI.BeginChangeCheck();
            _searchKeyword = EditorGUILayout.TextField(_searchKeyword, GUILayout.ExpandWidth(true));

            if (!string.IsNullOrEmpty(_searchKeyword) && GUILayout.Button("X", GUILayout.Width(20)))
            {
                _searchKeyword = "";
                GUI.FocusControl("");
            }

            if (EditorGUI.EndChangeCheck())
            {
                _selectedComponentIndex = 0;
                Repaint();
            }

            GUILayout.Space(5);

            if (FilterTypeNames(typeNames).Length == 0)
                EditorGUILayout.LabelField("No components found.", GUILayout.ExpandWidth(true));
            else
                _selectedComponentIndex = EditorGUILayout.Popup(_selectedComponentIndex, FilterTypeNames(typeNames),
                    GUILayout.ExpandWidth(true));

            EditorGUILayout.EndHorizontal();
        }

        private string[] FilterTypeNames(string[] typeNames)
        {
            if (string.IsNullOrEmpty(_searchKeyword)) return typeNames;
            var filteredTypeNames = typeNames.Where(typeName => typeName.ToLower().Contains(_searchKeyword.ToLower()))
                .ToList();

            if (_selectedComponentIndex >= filteredTypeNames.Count) _selectedComponentIndex = 0;

            return filteredTypeNames.ToArray();
        }

        private string[] GetComponentTypeNames()
        {
            return GetAvailableComponentTypes().Select(type => type.Name).ToArray();
        }

        private Type GetComponentTypeFromDropdown()
        {
            var componentTypes = GetAvailableComponentTypes();

            if (_selectedComponentIndex >= 0 && _selectedComponentIndex < componentTypes.Length)
                return componentTypes[_selectedComponentIndex];
            return null;
        }

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

        private void DrawRenamingTools()
        {
            EditorGUILayout.LabelField("Type:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
            _currentRenamingType = (RenamingType)EditorGUILayout.EnumPopup(_currentRenamingType);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            switch (_currentRenamingType)
            {
                case RenamingType.ReplaceCompletely:
                    EditorGUILayout.LabelField("Replace by:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
                    _renameTextValue = EditorGUILayout.TextField(_renameTextValue, GUILayout.ExpandWidth(true));
                    break;
                case RenamingType.ReplaceInName:
                    EditorGUILayout.LabelField("Replace:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
                    _renameTextValue = EditorGUILayout.TextField(_renameTextValue, GUILayout.ExpandWidth(true));

                    GUILayout.Space(FieldSpace);
                    EditorGUILayout.EndHorizontal();
                    GUILayout.Space(2.5f);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(FieldSpace);
                    EditorGUILayout.LabelField("By:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));

                    _renameTextValue2 = EditorGUILayout.TextField(_renameTextValue2, GUILayout.ExpandWidth(true));
                    break;
                case RenamingType.AddStart:
                case RenamingType.AddEnd:
                    EditorGUILayout.LabelField("Add:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
                    _renameTextValue = EditorGUILayout.TextField(_renameTextValue, GUILayout.ExpandWidth(true));
                    break;
                case RenamingType.RemoveStart:
                case RenamingType.RemoveEnd:
                    EditorGUILayout.LabelField("Count:", GUILayout.ExpandWidth(false), GUILayout.MaxWidth(75));
                    _removeCharacterCount =
                        EditorGUILayout.IntField(_removeCharacterCount, GUILayout.ExpandWidth(true));
                    break;
            }

            GUILayout.Space(FieldSpace);
            EditorGUILayout.EndHorizontal();
            GUILayout.Space(2.5f);

            GUILayout.Space(FieldSpace);
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(FieldSpace);

            if (GUILayout.Button("RENAME SELECTION", _skin.GetStyle(_editorData.addButtonStyle), GUILayout.Height(50),
                    GUILayout.ExpandWidth(true)))
            {
                if (objects.Length == 0) return;

                foreach (var selectedObject in objects) selectedObject.name = GetNewName(selectedObject.name);
            }
        }

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

        private bool IsMatchingNameRules(string goName)
        {
            switch (_currentSelectionNameType)
            {
                case SelectionNameType.Contains:
                    return goName.ToLower().Contains(_selectionNameValue);
                case SelectionNameType.ContainsExactly:
                    return goName.Contains(_selectionNameValue);
                case SelectionNameType.DoNotContain:
                    return !goName.Contains(_selectionNameValue);
                case SelectionNameType.Equal:
                    return string.Equals(goName, _selectionNameValue);
                case SelectionNameType.DoNotEqual:
                    return !string.Equals(goName, _selectionNameValue);
                case SelectionNameType.StartWith:
                    return goName.StartsWith(_selectionNameValue);
                case SelectionNameType.DoNotStartWith:
                    return !goName.StartsWith(_selectionNameValue);
                case SelectionNameType.EndWith:
                    return goName.EndsWith(_selectionNameValue);
                case SelectionNameType.DoNotEndWith:
                    return !goName.EndsWith(_selectionNameValue);
            }

            return false;
        }

        #region Enums Data

        private enum ToolCategory
        {
            Select,
            Rename
        }

        private ToolCategory _currentToolCategory;

        private enum SelectionType
        {
            Component,
            Name
        }

        private SelectionType _currentSelectionType;

        private enum SearchMode
        {
            Auto,
            Manual
        }

        private SearchMode _currentSearchMode;

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
    }
}