using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace EditorUI {
    /// <summary>
    /// Helper class for managing a toggleable shader property with saved state.
    /// </summary>
    public class SavedToggleShaderProperty {
        /// <summary>
        /// Key in the EditorPrefs for storing the property state.
        /// </summary>
        private readonly string prefsKey;

        /// <summary>
        /// Name of the shader variable.
        /// </summary>
        private readonly string variableName;

        /// <summary>
        /// Display name of the toggle.
        /// </summary>
        private readonly string displayName;

        /// <summary>
        /// Default state of the property.
        /// </summary>
        private readonly bool defaultState;

        /// <summary>
        /// Flag indicating whether to negate the state of the property.
        /// </summary>
        private readonly bool negateState;

        /// <summary>
        /// Gets or sets the current state of the property.
        /// </summary>
        public bool IsPropertyActive {
            get => EditorPrefs.GetBool(prefsKey, defaultState);
            set => EditorPrefs.SetBool(prefsKey, value);
        }

        /// <summary>
        /// Constructor for SavedToggleShaderProperty.
        /// </summary>
        /// <param name="baseKey">Base key for the EditorPrefs.</param>
        /// <param name="variableName">Name of the shader variable.</param>
        /// <param name="displayName">Display name of the toggle.</param>
        /// <param name="defaultState">Default state of the property.</param>
        /// <param name="negateState">Flag indicating whether to negate the state of the property.</param>
        public SavedToggleShaderProperty(string baseKey, string variableName, string displayName, bool defaultState = true, bool negateState = false) {
            prefsKey = baseKey + "." + variableName;
            this.variableName = variableName;
            this.displayName = displayName;
            this.defaultState = defaultState;
            this.negateState = negateState;
        }

        /// <summary>
        /// Creates a GUI element for the toggle property.
        /// </summary>
        /// <param name="root">Root VisualElement to attach the toggle to.</param>
        /// <param name="OnChange">Action to be invoked when the toggle value changes.</param>
        public void CreateGUI(VisualElement root, Action OnChange) {
            Toggle toggle = new Toggle(displayName) { value = IsPropertyActive };
            toggle.RegisterValueChangedCallback((evt) => {
                IsPropertyActive = evt.newValue;
                OnChange?.Invoke();
            });
            root.Add(toggle);
        }

        /// <summary>
        /// Draws the toggle property in an Editor window.
        /// </summary>
        /// <param name="rect">Position and size of the GUI element.</param>
        /// <returns>True if the property value changed, false otherwise.</returns>
        public bool Draw(Rect rect) {
            EditorGUI.BeginChangeCheck();

            bool newState = EditorGUI.Toggle(rect, displayName, IsPropertyActive);
            if (Event.current.type == EventType.MouseUp && rect.Contains(Event.current.mousePosition)) {
                newState = !IsPropertyActive;
            }

            if (EditorGUI.EndChangeCheck() || newState != IsPropertyActive) {
                IsPropertyActive = newState;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Sets the shader property in a MaterialPropertyBlock.
        /// </summary>
        /// <param name="propertyBlock">The MaterialPropertyBlock to set the property in.</param>
        public void SetProperty(MaterialPropertyBlock propertyBlock) {
            if (!negateState) {
                propertyBlock.SetFloat(variableName, IsPropertyActive ? 1 : 0);
            } else {
                propertyBlock.SetFloat(variableName, IsPropertyActive ? 0 : 1);
            }
        }
    }
}
