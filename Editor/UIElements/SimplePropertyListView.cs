using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorUI {
    /// <summary>
    /// A simple list view for SerializedProperty elements.
    /// </summary>
    public class SimplePropertyListView : SimpleListView {
        /// <summary>
        /// The SerializedProperty representing the list.
        /// </summary>
        public SerializedProperty listProperty;

        /// <summary>
        /// Action to bind a VisualElement to a SerializedProperty element at a given index.
        /// </summary>
        private Action<VisualElement, SerializedProperty, int> bindItemCustomAction;

        /// <summary>
        /// Action to handle selection change of a SerializedProperty element.
        /// </summary>
        private Action<SerializedProperty> selectionChangeCustomAction;

        /// <summary>
        /// Constructor for the SimplePropertyListView class.
        /// </summary>
        /// <param name="property">The SerializedProperty representing the list.</param>
        /// <param name="bindItem">Action to bind a VisualElement to a SerializedProperty element at a given index.</param>
        /// <param name="selectionChange">Action to handle selection change of a SerializedProperty element.</param>
        /// <param name="reorderable">Flag to enable or disable reordering of items.</param>
        /// <param name="showAddRemoveFooter">Flag to show or hide the add/remove footer.</param>
        /// <param name="itemRowContainer">Name of the item row container.</param>
        /// <param name="createRow">Action for creating a row in the list view.</param>
        /// <param name="addClicked">Action for handling the add button click event.</param>
        /// <param name="removeClicked">Action for handling the remove button click event.</param>
        /// <param name="doDefaultMake">Flag to indicate whether to use the default make item behavior.</param>
        public SimplePropertyListView(SerializedProperty property, Action<VisualElement, SerializedProperty, int> bindItem, Action<SerializedProperty> selectionChange = null, bool reorderable = true, bool showAddRemoveFooter = true, string itemRowContainer = DEFAULT_ITEMROW_CONTAINER_NAME, Action<VisualElement> createRow = null, Action addClicked = null, Action removeClicked = null, bool doDefaultMake = false) : base(reorderable, showAddRemoveFooter, itemRowContainer, createRow, addClicked, removeClicked, doDefaultMake) {
            listProperty = property;
            bindItemCustomAction = bindItem;
            selectionChangeCustomAction = selectionChange;

            if (bindItemCustomAction != null) {
                this.bindItem = BindItem;
            }

            if (selectionChange != null) {
                selectedIndicesChanged -= SelectionChanged;
                selectedIndicesChanged += SelectionChanged;
            }

            bindingPath = property.propertyPath;
        }

        /// <summary>
        /// Adds a new item to the list.
        /// </summary>
        protected override void AddClicked() {
            listProperty.serializedObject.Update();
            listProperty.arraySize++;
            listProperty.serializedObject.ApplyModifiedProperties();
            base.AddClicked();
        }

        /// <summary>
        /// Removes the selected item from the list.
        /// </summary>
        protected override void RemoveClicked() {
            listProperty.serializedObject.Update();
            if (selectedIndex > 0) {
                listProperty.DeleteArrayElementAtIndex(selectedIndex);
            } else {
                listProperty.arraySize--;
            }
            listProperty.serializedObject.ApplyModifiedProperties();
            base.RemoveClicked();
        }

        /// <summary>
        /// Handles the selection change event.
        /// </summary>
        /// <param name="indices">The selected indices.</param>
        private void SelectionChanged(IEnumerable<int> indices) {
            if (selectedIndex >= 0) {
                selectionChangeCustomAction?.Invoke(listProperty.GetArrayElementAtIndex(selectedIndex).Copy());
            } else {
                selectionChangeCustomAction?.Invoke(null);
            }
        }

        /// <summary>
        /// Binds a VisualElement to a SerializedProperty element at a given index.
        /// </summary>
        /// <param name="rowItem">The VisualElement representing the item row.</param>
        /// <param name="index">The index of the element in the list.</param>
        private void BindItem(VisualElement rowItem, int index) {
            bindItemCustomAction?.Invoke(rowItem, listProperty.GetArrayElementAtIndex(index), index);
        }
    }
}

