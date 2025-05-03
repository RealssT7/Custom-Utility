using System.Collections;
using CustomUtility.ConsoleUtility.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtility.ConsoleUtility
{
    public class ConsoleUtility : MonoBehaviour
    {
        private UIDocument _uiDocument;
        private VisualElement _root;
        private TextField _inputField;
        private ScrollView _outputArea;
        private bool _isVisible;

        private void Awake()
        {
            _uiDocument = gameObject.AddComponent<UIDocument>();

            var console = Resources.Load<VisualTreeAsset>("ConsoleUtilityUI");
            var settings = Resources.Load<PanelSettings>("ConsoleUtilityPanel");

            if (console == null || settings == null)
            {
                Debug.LogError("Console UI or PanelSettings not found in Resources folder.");
                enabled = false;
                return;
            }

            _uiDocument.visualTreeAsset = console;
            _uiDocument.panelSettings = settings;

            _root = _uiDocument.rootVisualElement;
            _inputField = _root.Q<TextField>("ConsoleInput");
            _outputArea = _root.Q<ScrollView>("ConsoleOutput");

            _inputField.RegisterCallback<KeyDownEvent>(OnKeyDown);
            _root.style.display = DisplayStyle.None;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                ToggleConsole();
            }
        }

        private void ToggleConsole()
        {
            _isVisible = !_isVisible;
            _root.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (_isVisible) StartCoroutine(FocusInputField());
            else _outputArea.Clear();
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode is KeyCode.Return or KeyCode.KeypadEnter)
            {
                RunCommand(_inputField.text);
                StartCoroutine(FocusInputField());
                evt.StopPropagation(); 
            }
        }

        private void RunCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                AppendToOutput("Error: Input field is empty. Please enter a command.", "textError");
                return;
            }

            AppendToOutput($"> {input}");

            var result = CommandParser.Execute(input);

            if (result.Contains("Unknown command") || result.Contains("Invalid argument") || result.Contains("No command")) AppendToOutput(result, "textError");
            else AppendToOutput(result, "textSuccess");
        }

        private void AppendToOutput(string message, string styleClass = "textInfo")
        {
            var label = new Label(message);
            label.AddToClassList(styleClass);
            _outputArea.Add(label);
        }

        private IEnumerator FocusInputField()
        {
            _inputField.value = string.Empty;
            _inputField.Blur();
            yield return new WaitForEndOfFrame();
            _inputField.Focus();
            _outputArea.scrollOffset = new Vector2(0, _outputArea.contentContainer.layout.height * 2);
        }
    }
}
