using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI {
    public class ModalInputPopup : EditorWindow {
        //Visual Elements
        TextField keyInput;
        Button okButton;

        //Defaults
        Vector2 windowSize = new Vector2(250, 70);

        //Result
        Result result = new Result();

        //Properties
        string description;
        string okText;
        string cancelText;
        string defaultValue;
        Func<string, bool> inputCheck;

        /// <summary>
        /// Create an Modal Input Popup
        /// </summary>
        /// <param name="header">Header of the Popup</param>
        /// <param name="description">Description in the Popup</param>
        /// <param name="okText">Text for the Ok Button</param>
        /// <param name="cancelText">Text for the Cancel Button</param>
        /// <param name="inputCheck">Function that should called on input change to check the typed string with</param>
        public static Result ShowModalPopup(string header, string description, string okText = "Ok", string cancelText = "Cancel", string defaultValue = "", Func<string, bool> inputCheck = null, Vector2? windowSize = null) {
            ModalInputPopup window = CreateInstance<ModalInputPopup>();
            window.description = description;
            window.okText = okText;
            window.cancelText = cancelText;
            window.inputCheck = inputCheck;
            window.defaultValue = defaultValue;
            if (windowSize != null) {
                window.windowSize = windowSize.Value;
            }
            window.maxSize = window.windowSize;
            window.minSize = window.windowSize;
            window.titleContent = new GUIContent(header);
            window.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - window.windowSize / 2, window.windowSize);
            window.ShowModalUtility();

            return window.result;
        }

        void CreateGUI() {
            rootVisualElement.style.marginBottom = 2;
            rootVisualElement.style.marginLeft = 2;
            rootVisualElement.style.marginRight = 2;
            rootVisualElement.style.marginTop = 5;

            Label descriptionLbl = new Label(description);
            descriptionLbl.style.marginLeft = 2;

            keyInput = new TextField();
            keyInput.value = defaultValue;
            keyInput.RegisterValueChangedCallback((evt) => ValidateInput(evt.newValue));
            keyInput.RegisterCallback<KeyDownEvent>(e => {
                if (e.keyCode == KeyCode.Return) {
                    OkButton_clicked();
                } else if (e.keyCode == KeyCode.Escape && string.IsNullOrEmpty(keyInput.text)) {
                    CancelButton_clicked();
                }
            });

            okButton = new Button(OkButton_clicked);
            okButton.text = okText;
            okButton.style.flexGrow = 1;
            okButton.style.width = new StyleLength(100);
            okButton.SetEnabled(false);

            Button cancelButton = new Button(CancelButton_clicked);
            cancelButton.text = cancelText;
            cancelButton.style.flexGrow = 1;
            cancelButton.style.width = new StyleLength(100);

            VisualElement buttonContainer = new VisualElement();
            buttonContainer.style.flexDirection = FlexDirection.Row;
            buttonContainer.Add(okButton);
            buttonContainer.Add(cancelButton);

            rootVisualElement.Add(descriptionLbl);
            rootVisualElement.Add(keyInput);
            rootVisualElement.Add(buttonContainer);

            keyInput.Focus();
            ValidateInput(defaultValue);
        }

        private void ValidateInput(string value) {
            if (string.IsNullOrEmpty(value)) {
                okButton.SetEnabled(false);
                return;
            }

            if (inputCheck != null && !inputCheck.Invoke(value)) {
                okButton.SetEnabled(false);
                return;
            }

            okButton.SetEnabled(true);
        }

        private void CancelButton_clicked() {
            result.output = string.Empty;
            result.success = false;
            Close();
        }

        private void OkButton_clicked() {
            result.output = keyInput.text;
            result.success = true;
            Close();
        }

    }

    public class Result {
        public bool success = false;
        public string output = string.Empty;
    }
}