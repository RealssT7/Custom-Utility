using System;
using System.Collections;
using CustomUtility.ConsoleUtility.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CustomUtility.ConsoleUtility
{
    /// <summary>
    ///     A utility class for managing a console interface in Unity.
    ///     Provides functionality to display a console, process commands, and display output.
    /// </summary>
    public class ConsoleUtility : MonoBehaviour
    {
        // Paths to UI assets and settings in the Resources folder.
        private const string ConsoleUIPath = "ConsoleUtilityUI";
        private const string PanelSettingsPath = "ConsoleUtilityPanel";

        // Names of UI elements in the console interface.
        private const string InputFieldName = "ConsoleInput";
        private const string OutputFieldName = "ConsoleOutput";

        // Styles for different types of output messages.
        private const string ErrorStyle = "textError";
        private const string SuccessStyle = "textSuccess";
        private const string InfoStyle = "textInfo";

        // Key bindings for toggling and submitting commands in the console.
        private const KeyCode ToggleKey = KeyCode.BackQuote;
        private const KeyCode SubmitKey = KeyCode.Return;

        // Private fields for managing the console's state and UI elements.
        private bool _isVisible;
        private VisualElement _root;
        private UIDocument _uiDocument;
        private static ScrollView _outputArea;
        private TextField _inputField;

        /// <summary>
        ///     Unity's Awake method, called when the script instance is being loaded.
        ///     Initializes the console UI and registers necessary callbacks.
        /// </summary>
        private void Awake()
        {
            if (!InitializeUI()) return;
            RegisterCallbacks();
            HideConsole();
        }

        /// <summary>
        ///     Unity's Update method, called once per frame.
        ///     Toggles the console visibility when the toggle key is pressed.
        /// </summary>
        private void Update()
        {
            if (Input.GetKeyDown(ToggleKey)) ToggleConsole();
        }

        /// <summary>
        ///     Initializes the console UI by loading assets and settings from the Resources folder.
        /// </summary>
        /// <returns>True if initialization is successful, false otherwise.</returns>
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

        /// <summary>
        ///     Registers event callbacks for the console input field.
        /// </summary>
        private void RegisterCallbacks()
        {
            _inputField.RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        /// <summary>
        ///     Toggles the visibility of the console.
        /// </summary>
        private void ToggleConsole()
        {
            _isVisible = !_isVisible;
            _root.style.display = _isVisible ? DisplayStyle.Flex : DisplayStyle.None;

            if (_isVisible) StartCoroutine(FocusInputField());
            else ClearConsole();
        }

        public static void ClearConsole()
        {
            _outputArea.Clear();
            CommandParser.ResetParser();
        }

        /// <summary>
        ///     Hides the console by setting its visibility to none.
        /// </summary>
        private void HideConsole()
        {
            _root.style.display = DisplayStyle.None;
            _isVisible = false;
        }

        /// <summary>
        ///     Handles the KeyDown event for the input field.
        ///     Processes the command when the submit key is pressed.
        /// </summary>
        /// <param name="evt">The KeyDownEvent triggered by the input field.</param>
        private void OnKeyDown(KeyDownEvent evt)
        {
            if (evt.keyCode != SubmitKey) return;

            ProcessCommand(_inputField.text);
            StartCoroutine(FocusInputField());
            evt.StopPropagation();
        }

        /// <summary>
        ///     Processes a command entered the console input field.
        /// </summary>
        /// <param name="input">The command string entered by the user.</param>
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

        /// <summary>
        ///     Appends a message to the console output area with the specified style.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="style">The style to apply to the message (default is InfoStyle).</param>
        private void AppendToOutput(string message, string style = InfoStyle)
        {
            var label = new Label(message);
            label.AddToClassList(style);
            _outputArea.Add(label);
        }

        /// <summary>
        ///     Determines the appropriate style for a command result message.
        /// </summary>
        /// <param name="result">The result string of the command execution.</param>
        /// <returns>The style to apply to the result message.</returns>
        private static string GetOutputStyle(string result)
        {
            if (result.Contains("Unknown", StringComparison.CurrentCultureIgnoreCase) ||
                result.Contains("Invalid", StringComparison.CurrentCultureIgnoreCase) ||
                result.Contains("Error", StringComparison.CurrentCultureIgnoreCase) ||
                result.Contains("Mismatch", StringComparison.CurrentCultureIgnoreCase) ||
                result.Contains("Failed", StringComparison.CurrentCultureIgnoreCase)) return ErrorStyle;
            return SuccessStyle;
        }

        /// <summary>
        ///     Focuses the input field and scrolls the output area to the bottom.
        /// </summary>
        /// <returns>An IEnumerator for coroutine execution.</returns>
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