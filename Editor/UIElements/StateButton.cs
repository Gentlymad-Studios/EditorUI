using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorUI {
    /// <summary>
    /// Custom button with state functionality.
    /// </summary>
    public class StateButton : Button {
        /// <summary>
        /// Key for storing the state in the EditorPrefs.
        /// </summary>
        private string stateEditorPrefsKey;

        /// <summary>
        /// Current state of the button.
        /// </summary>
        private bool state;

        /// <summary>
        /// Action to be performed after the button is clicked.
        /// </summary>
        private Action afterClicked;

        /// <summary>
        /// CSS class to apply when the button is active.
        /// </summary>
        private string buttonActiveClass;

        /// <summary>
        /// Gets the current state of the button.
        /// </summary>
        public bool State => state;

        /// <summary>
        /// Constructor for the StateButton class.
        /// </summary>
        /// <param name="stateEditorPrefsKey">Key for storing the state in the EditorPrefs.</param>
        /// <param name="buttonLabel">Label text of the button.</param>
        /// <param name="buttonActiveClass">CSS class to apply when the button is active.</param>
        /// <param name="afterClicked">Action to be performed after the button is clicked.</param>
        /// <param name="defaultStateValue">Default state value of the button.</param>
        public StateButton(string stateEditorPrefsKey, string buttonLabel, string buttonActiveClass, Action afterClicked = null, bool defaultStateValue = true) {
            this.stateEditorPrefsKey = stateEditorPrefsKey;
            this.afterClicked = afterClicked;
            this.buttonActiveClass = buttonActiveClass;

            text = buttonLabel;
            clicked -= OnButtonClicked;
            clicked += OnButtonClicked;

            OnButtonClickedBase(EditorPrefs.GetBool(stateEditorPrefsKey, defaultStateValue));
        }

        /// <summary>
        /// Event handler for button click event.
        /// </summary>
        private void OnButtonClicked() {
            OnButtonClickedBase(!state);
        }

        /// <summary>
        /// Sets the button state and updates its appearance.
        /// </summary>
        /// <param name="stateToSet">State value to set.</param>
        private void OnButtonClickedBase(bool stateToSet) {
            state = stateToSet;

            if (state) {
                AddToClassList(buttonActiveClass);
            } else {
                RemoveFromClassList(buttonActiveClass);
            }

            EditorPrefs.SetBool(stateEditorPrefsKey, state);
            afterClicked?.Invoke();
        }
    }
}
