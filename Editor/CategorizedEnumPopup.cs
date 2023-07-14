using UnityEngine;
using UnityEditor.IMGUI.Controls;
using System;
using UnityEditor;

namespace CategorizedEnum {
    /// <summary>
    /// The popup window for our Categorized Enum Drawer.
    /// This class manages the tree view as well as everything window related.
    /// </summary>
    public class CategorizedEnumPopup : PopupWindowContent {
        // We are using SerializeField here to make sure view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If omitting the attribute then the state just survives assembly reloading 
        // (i.e. it still gets serialized/deserialized)
        [SerializeField]
        private readonly TreeViewState treeViewState;

        // The TreeView is not serializable it should be reconstructed from the tree data.
        private CategorizedEnumTreeView treeView;
        private SearchField searchField;

        // Styles & View
        private GUIStyle layoutGroup = null;
        private bool initializedStyles = false;
        private readonly float windowWidth = 200;
        private readonly float windowHeight = 300;
        public static Action repaintAction = null;
        private bool firstRun = true;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="delimiter">The character to retrieve the categories with.</param>
        /// <param name="enumNames">The string array of enum names.</param>
        /// <param name="currentEnumIndex">The currently selected index.</param>
        /// <param name="onSelectionChange">Action called when the selection changed.</param>
        public CategorizedEnumPopup(char delimiter, bool showFullPath, string[] enumNames, int currentEnumIndex, Action<int> onSelectionChange, float windowWidth, string frameItem = "") {
            firstRun = true;

            // Check if we already had a serialized view state (state 
            // that survived assembly reloading)
            if (treeViewState == null) {
                treeViewState = new TreeViewState();
            }

            // create & setup the treeview
            treeView = new CategorizedEnumTreeView(treeViewState, showFullPath, delimiter, onSelectionChange, enumNames, currentEnumIndex, frameItem);

            // setup the search field
            searchField = new SearchField();
            searchField.downOrUpArrowKeyPressed += treeView.SetFocus;

            // set the window width that should be used.
            this.windowWidth = windowWidth;
        }

        /// <summary>
        /// Creates a CategorizedEnumPopup window and displays it.
        /// </summary>
        /// <param name="rect">The initial rect to build the window</param>
        /// <param name="delimiter">The character to retrieve the categories with.</param>
        /// <param name="enumNames">The string array of enum names.</param>
        /// <param name="currentEnumIndex">The currently selected index.</param>
        /// <param name="onSelectionChange">Action called when the selection changed.</param>
        public static void Show(Rect rect, char delimiter, bool showFullPath, string[] enumNames, int currentEnumIndex, Action<int> onSelectionChange, string frameItem = "") {
            CategorizedEnumPopup win = new CategorizedEnumPopup(delimiter, showFullPath, enumNames, currentEnumIndex, onSelectionChange, rect.width, frameItem);
            repaintAction = () => win.editorWindow.Repaint();
            PopupWindow.Show(rect, win);
        }

        /// <summary>
        /// Setup custom GUI Styles.
        /// </summary>
        private void SetupStyles() {
            if (!initializedStyles) {
                layoutGroup = new GUIStyle();
                layoutGroup.margin = new RectOffset(5, 5, 5, 5);
                initializedStyles = true;
            }
        }

        /// <summary>
        /// Set the window size.
        /// </summary>
        /// <returns></returns>
        public override Vector2 GetWindowSize() {
            return new Vector2(windowWidth, windowHeight);
        }

        /// <summary>
        /// Called when the window is opened.
        /// </summary>
        public override void OnOpen() {
            base.OnOpen();
            // Force a repaint every frame to be responsive to mouse hover.
            EditorApplication.update += Repaint;
        }

        /// <summary>
        /// Called when the window is closed.
        /// </summary>
        public override void OnClose() {
            base.OnClose();
            EditorApplication.update -= Repaint;
        }

        /// <summary>
        /// Repaint the window.
        /// </summary>
        public static void Repaint() {
            repaintAction?.Invoke();
        }

        /// <summary>
        /// Draws the UI of the window.
        /// </summary>
        /// <param name="rect"></param>
        public override void OnGUI(Rect rect) {
            // setup GUI styles
            SetupStyles();
            EditorGUILayout.BeginVertical(layoutGroup);
            GUI.SetNextControlName("searchString");
            treeView.searchString = searchField.OnGUI(treeView.searchString);
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            treeView.OnGUI(GUILayoutUtility.GetRect(0, 100000, 0, 100000));
            EditorGUILayout.EndVertical();
            if (firstRun) {
                GUI.FocusControl("searchString");
                firstRun = false;
            }
        }
    }
}