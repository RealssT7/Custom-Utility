using UnityEngine;

namespace CustomUtility.ConsoleUtility.Helper
{
    /// <summary>
    ///     A helper class that displays a label above a GameObject to indicate its command target index.
    /// </summary>
    public class CommandTargetIndicator : MonoBehaviour
    {
        /// <summary>
        ///     The index of the command target, displayed in the label.
        /// </summary>
        public int Index;

        /// <summary>
        ///     The GameObject representing the label.
        /// </summary>
        private GameObject _labelObject;

        /// <summary>
        ///     The main camera in the scene, used to orient the label.
        /// </summary>
        private Camera _mainCamera;

        // Constants for label properties
        private const string LabelName = "CommandLabel"; // The name of the label GameObject.
        private const float LabelHeight = 2f; // The height of the label above the GameObject.
        private const int FontSize = 12; // The font size of the label text.
        private const TextAnchor LabelAnchor = TextAnchor.MiddleCenter; // The anchor point of the label text.
        private const TextAlignment LabelAlignment = TextAlignment.Center; // The alignment of the label text.
        private const float CharacterSize = 1f; // The size of the label text characters.

        private void Start() => CreateLabel();

        private void Update() => OrientLabelTowardCamera();

        /// <summary>
        ///     Creates a label GameObject and attaches it to the current GameObject.
        /// </summary>
        private void CreateLabel()
        {
            _labelObject = new GameObject(LabelName);
            _labelObject.transform.SetParent(transform);
            _labelObject.transform.localPosition = Vector3.up * LabelHeight;

            var textMesh = _labelObject.AddComponent<TextMesh>();
            textMesh.text = $"[{Index}]";
            textMesh.color = Color.cyan;
            textMesh.fontSize = FontSize;
            textMesh.anchor = LabelAnchor;
            textMesh.alignment = LabelAlignment;
            textMesh.characterSize = CharacterSize;

            _mainCamera = Camera.main;
        }

        /// <summary>
        ///     Orients the label to face the main camera.
        /// </summary>
        private void OrientLabelTowardCamera()
        {
            if (_labelObject == null || _mainCamera == null) return;

            var direction = _labelObject.transform.position - _mainCamera.transform.position;
            direction.y = 0;
            _labelObject.transform.rotation = Quaternion.LookRotation(direction);
        }

        /// <summary>
        ///     Destroys the label GameObject and this component.
        /// </summary>
        public void DestroyLabel()
        {
            if (_labelObject != null)
                Destroy(_labelObject);

            Destroy(this);
        }
    }
}