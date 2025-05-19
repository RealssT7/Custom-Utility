using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using CustomUtility.ConsoleUtility.Attributes;
using CustomUtility.ConsoleUtility.Editor;
using CustomUtility.ConsoleUtility.Input;

namespace CustomUtility.ConsoleUtility
{
    /// <summary>
    ///     A utility class for managing a console interface in Unity using UGUI prefabs.
    ///     Provides functionality to display a console, process commands, and display styled output.
    ///     This version uses a prefab-based setup for greater designer flexibility.
    /// </summary>
    public class ConsoleUtility : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject consolePanel;
        [SerializeField] private InputField inputField;
        [SerializeField] private Text outputText;
        [SerializeField] private ConsoleSettings settings;
        private bool _isVisible;
        private const string InitialText = "To view available commands, type \"Help\". \n";
        private readonly StringBuilder _outputBuilder = new();
        private CustomConsole_ActionMap _inputs;
        private Action<InputAction.CallbackContext> _toggleCallback;
        
        [Header("Console Style Configuration")]
        [SerializeField] private ConsoleOutputStyle defaultStyle;
        [SerializeField] private ConsoleOutputStyle inputStyle;
        [SerializeField] private ConsoleOutputStyle warningStyle;
        [SerializeField] private ConsoleOutputStyle errorStyle;
        [SerializeField] private List<ConsoleOutputStyle> outputStyles;
        
        /// <summary>
        ///     Unity's Awake method, called when the script instance is being loaded.
        ///     Validates UI references and hides the console initially.
        /// </summary>
        private void Awake()
        {
            consolePanel.SetActive(false);
            inputField.onSubmit.AddListener(OnCommandSubmit);
            CommandRegistry.Initialize(settings);
            ClearConsole();
        }
        
        private void OnEnable()
        {
            _inputs = new CustomConsole_ActionMap();
            _toggleCallback = _ => ToggleConsole();
            _inputs.CustomConsole.ToggleWindow.performed += _toggleCallback;
            _inputs.Enable();
        }

        private void OnDisable()
        {
            _inputs.CustomConsole.ToggleWindow.performed -= _toggleCallback;
            _inputs.Disable();
        }
        
        /// <summary>
        ///     Toggles the visibility of the console.
        /// </summary>
        private void ToggleConsole()
        {
            _isVisible = !_isVisible;
            consolePanel.SetActive(_isVisible);

            if (!_isVisible) return;
            
            FocusInputField();
            ClearConsole();
        }

        /// <summary>
        ///     Handles submission of commands from the input field.
        /// </summary>
        /// <param name="input">The command string entered by the user.</param>
        private void OnCommandSubmit(string input)
        {
            var result = CommandParser.Execute(input);
            AppendToOutput($"> {input}", inputStyle.TextColor);
            AppendToOutput(result, GetOutputStyle(result).TextColor);
            FocusInputField();
        }

        /// <summary>
        ///     Appends a message to the console output area with the specified style.
        /// </summary>
        /// <param name="message">The message to display.</param>
        /// <param name="style">The style to apply to the message.</param>
        private void AppendToOutput(string message, Color style)
        {
            var colorHex = ColorUtility.ToHtmlStringRGB(style);
            _outputBuilder.AppendLine($"<color=#{colorHex}>{message}</color>");
            _outputBuilder.AppendLine();
            outputText.text = _outputBuilder.ToString();
        }

        /// <summary>
        ///     Determines the appropriate style for a command result message.
        /// </summary>
        /// <param name="result">The result string of the command execution.</param>
        /// <returns>The style to apply to the result message.</returns>
        private ConsoleOutputStyle GetOutputStyle(string result)
        {
            foreach (var style in outputStyles.Where(style =>
                         style.Keywords.Any(word => result.Contains(word, StringComparison.OrdinalIgnoreCase)))) return style;
            return defaultStyle;
        }
        /// <summary>
        /// Clears the input field and focus input field.
        /// </summary>
        private void FocusInputField()
        {
            inputField.text = string.Empty;
            inputField.ActivateInputField();
        }

        /// <summary>
        ///     Clears all output messages from the console.
        /// </summary>
        [ConsoleCommand("clear", "Clears the console output.")]
        private string ClearConsole()
        {
            _outputBuilder.Clear();
            _outputBuilder.AppendLine(InitialText);
            outputText.text = InitialText;
            CommandParser.ResetParser();
            return InitialText;
        }
    }
}