using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI {
    public class ModalInputPopup : EditorWindow {
        ModalInputPopup window;

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
        string defaultValue = string.Empty;
        Func<string, bool> inputCheck;

        public string Header {
            set {
                window.titleContent.text = value;
            }
        }
        public string Description {
            set {
                description = value;
            }
        }
        public string OkText {
            set {
                okText = value;
            }
        }
        public string CancelText {
            set {
                cancelText = value;
            }
        }
        public string DefaultValue {
            set {
                defaultValue = value;
            }
        }
        public Func<string, bool> InputCheck {
            set {
                inputCheck = value;
            }
        }
        public Vector2 WindowSize {
            set {
                window.minSize = value;
                window.maxSize = value;
                window.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, window.minSize);
            }
        }

        /// <summary>
        /// Create an Modal Input Popup
        /// </summary>
        /// <param name="header">Header of the Popup</param>
        /// <param name="description">Description in the Popup</param>
        /// <param name="okText">Text for the Ok Button</param>
        /// <param name="cancelText">Text for the Cancel Button</param>
        /// <param name="inputCheck">Function that should called on input change to check the typed string with</param>
        public ModalInputPopup(string header, string description, string okText = "Ok", string cancelText = "Cancel", Func<string, bool> inputCheck = null) {
            window = CreateInstance<ModalInputPopup>();
            window.description = description;
            window.okText = okText;
            window.cancelText = cancelText;
            window.inputCheck = inputCheck;
            window.maxSize = windowSize;
            window.minSize = windowSize;
            window.titleContent = new GUIContent(header);
            window.position = new Rect(EditorGUIUtility.GetMainWindowPosition().center - windowSize / 2, windowSize);
        }

        /// <summary>
        /// Opens the Popup and returns the Result
        /// </summary>
        /// <returns></returns>
        public Result ShowModalPopup() {
            ShowModalUtility();
            return result;
        }

        void CreateGUI() {
            rootVisualElement.style.marginBottom = 2;
            rootVisualElement.style.marginLeft = 2;
            rootVisualElement.style.marginRight = 2;
            rootVisualElement.style.marginTop = 5;

            Label descriptionLbl = new Label(description);
            descriptionLbl.style.marginLeft = 2;

            keyInput = new TextField(defaultValue);
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