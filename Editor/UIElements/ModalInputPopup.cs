using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI {
    public class ModalInputPopup : EditorWindow {
        TextField keyInput;
        Button okButton;
        string description;
        string okText;
        string cancelText;
        Result result = new Result();
        Func<string, bool> inputCheck;
        static Vector2 windowSize = new Vector2(250, 70);

        void CreateGUI() {
            rootVisualElement.style.marginBottom = 2;
            rootVisualElement.style.marginLeft = 2;
            rootVisualElement.style.marginRight = 2;
            rootVisualElement.style.marginTop = 5;

            Label descriptionLbl = new Label(description);
            descriptionLbl.style.marginLeft = 2;

            keyInput = new TextField();
            keyInput.RegisterValueChangedCallback(KeyInput_changed);
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
        }

        private void KeyInput_changed(ChangeEvent<string> evt) {
            if (string.IsNullOrEmpty(evt.newValue)) {
                okButton.SetEnabled(false);
                return;
            }

            if (inputCheck != null && !inputCheck.Invoke(evt.newValue)) {
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

        public static Result ShowModal(string header, string description, string okText = "Ok", string cancelText = "Cancel", Func<string, bool> inputCheck = null) {
            ModalInputPopup window = CreateInstance<ModalInputPopup>();
            window.description = description;
            window.okText = okText;
            window.cancelText = cancelText;
            window.inputCheck = inputCheck;
            window.maxSize = windowSize;
            window.minSize = windowSize;
            window.titleContent = new GUIContent(header);
            window.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
            window.ShowModalUtility();

            return window.result;
        }
    }

    public class Result {
        public bool success = false;
        public string output = string.Empty;
    }
}