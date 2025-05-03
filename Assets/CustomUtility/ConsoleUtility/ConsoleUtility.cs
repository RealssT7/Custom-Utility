using System.Collections;
using CustomUtility.ConsoleUtility.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtility.ConsoleUtility
{
    public class ConsoleUtility : MonoBehaviour
    {
        private const string ConsoleUIPath = "ConsoleUtilityUI";
        private const string PanelSettingsPath = "ConsoleUtilityPanel";
        private const string InputFieldName = "ConsoleInput";
        private const string OutputFieldName = "ConsoleOutput";
        private const string ErrorStyle = "textError";
        private const string SuccessStyle = "textSuccess";
        private const string InfoStyle = "textInfo";
        private const KeyCode ToggleKey = KeyCode.BackQuote;
        private const KeyCode SubmitKey = KeyCode.Return;
        
        private bool _isVisible;
        private TextField _inputField;
        private ScrollView _outputArea;
        private VisualElement _root;
        private UIDocument _uiDocument;

        private void Awake()
        {
            if (!InitializeUI()) return;
            RegisterCallbacks();
            HideConsole();
        }

        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey)) ToggleConsole();
        }

        private bool InitializeUI()
        {
            _uiDocument = gameObject.AddComponent<UIDocument>();

            var uiAsset = Resources.Load<VisualTreeAsset>(ConsoleUIPath);
            var panelSettings = Resources.Load<PanelSettings>(PanelSettingsPath);

            if (uiAsset == null || panelSettings == null)
            {
                Debug.LogError("Missing Console UI or PanelSettings in Resources.");
                enabled = false;
                return false;
            }

            _uiDocument.visualTreeAsset = uiAsset;
            _uiDocument.panelSettings = panelSettings;
            _root = _uiDocument.rootVisualElement;
            _inputField = _root.Q<TextField>(InputFieldName);
            _outputArea = _root.Q<ScrollView>(OutputFieldName);

            return true;
        }

        private void RegisterCallbacks()
        {
            _inputField.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void ToggleConsole()
        {
            _isVisible = !_isVisible;
            _root.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (_isVisible) StartCoroutine(FocusInputField());
            else _outputArea.Clear();
        }

        private void HideConsole()
        {
            _root.style.display = DisplayStyle.None;
            _isVisible = false;
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != SubmitKey) return;

            ProcessCommand(_inputField.text);
            StartCoroutine(FocusInputField());
            evt.StopPropagation();
        }

        private void ProcessCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                AppendToOutput("Error: Input field is empty. Please enter a command.", ErrorStyle);
                return;
            }

            var result = CommandParser.Execute(input);
            var style = GetOutputStyle(result);

            AppendToOutput($"> {input}");
            AppendToOutput(result, style);
        }

        private void AppendToOutput(string message, string style = InfoStyle)
        {
            var label = new Label(message);
            label.AddToClassList(style);
            _outputArea.Add(label);
        }

        private static string GetOutputStyle(string result)
        {
            if (result.Contains("Unknown command") || result.Contains("Invalid argument")) return ErrorStyle;
            return SuccessStyle;
        }

        private IEnumerator FocusInputField()
        {
            _inputField.value = string.Empty;
            _inputField.Blur();
            yield return new WaitForEndOfFrame();
            _inputField.Focus();

            var scrollHeight = _outputArea.contentContainer.layout.height * 2;
            _outputArea.scrollOffset = new Vector2(0, scrollHeight);
        }
    }
}