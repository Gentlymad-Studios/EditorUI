using System;
using UnityEditor;
using UnityEngine.UIElements;

namespace EditorUI {
    /// <summary>
    /// Base class for a simple list view.
    /// </summary>
    public abstract class SimpleListView : ListView {
        /// <summary>
        /// Name of the item row container.
        /// </summary>
        private readonly string itemRowContainer = DEFAULT_ITEMROW_CONTAINER_NAME;

        /// <summary>
        /// Default name for the item row container.
        /// </summary>
        protected const string DEFAULT_ITEMROW_CONTAINER_NAME = nameof(SimpleListView) + "_" + nameof(itemRowContainer);

        /// <summary>
        /// Constant value for the handle bars partial GUID.
        /// </summary>
        private const string HANDLE_BARS_PARTIAL_GUID = "cef02cc912d280d4cb3ea263d3012dc7";

        /// <summary>
        /// Partial to identifiy a list view in ui elements
        /// </summary>
        private const string LISTVIEW_IDENTIFIER_PARTIAL = "unity-list-view__";

        /// <summary>
        /// Event for when an item is added.
        /// </summary>
        public Action addClicked;

        /// <summary>
        /// Event for when an item is removed.
        /// </summary>
        public Action removeClicked;

        /// <summary>
        /// Event for creating a row in the list view.
        /// </summary>
        public Action<VisualElement> createRow;

        /// <summary>
        /// Static variable for the handle bars partial.
        /// </summary>
        private static VisualTreeAsset _handleBarsPartial = null;

        /// <summary>
        /// Property for accessing the handle bars partial.
        /// </summary>
        public static VisualTreeAsset HandleBarsPartial {
            get {
                if (_handleBarsPartial == null) {
                    _handleBarsPartial = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(AssetDatabase.GUIDToAssetPath(HANDLE_BARS_PARTIAL_GUID));
                }
                return _handleBarsPartial;
            }
        }

        /// <summary>
        /// Constructor for SimpleListView.
        /// </summary>
        /// <param name="reorderable">Flag to enable or disable reordering of items.</param>
        /// <param name="showAddRemoveFooter">Flag to show or hide the add/remove footer.</param>
        /// <param name="itemRowContainer">Name of the item row container.</param>
        /// <param name="createRow">Action for creating a row in the list view.</param>
        /// <param name="addClicked">Action for handling the add button click event.</param>
        /// <param name="removeClicked">Action for handling the remove button click event.</param>
        /// <param name="doDefaultMake">Flag to indicate whether to use the default make item behavior.</param>
        public SimpleListView(bool reorderable = true, bool showAddRemoveFooter = true, string itemRowContainer = DEFAULT_ITEMROW_CONTAINER_NAME, Action<VisualElement> createRow = null, Action addClicked = null, Action removeClicked = null, bool doDefaultMake = false) {
            // Set flags
            this.reorderable = reorderable;
            this.showAddRemoveFooter = showAddRemoveFooter;
            this.itemRowContainer = itemRowContainer;

            // Set up events
            this.addClicked = addClicked;
            this.removeClicked = removeClicked;
            this.createRow = createRow;

            showBorder = true;
            if (!doDefaultMake) {
                makeItem = MakeItem;
            }
            selectionType = SelectionType.Single;
            reorderMode = ListViewReorderMode.Simple;
            showBoundCollectionSize = false;

            // Your ListView needs to take all the remaining space
            style.flexGrow = 1;

            // Overwrite default button behaviors due to bugs in ListView code
            schedule.Execute(() => {
                virtualizationMethod = CollectionVirtualizationMethod.DynamicHeight;
                Button removeButton = this.Q<Button>(LISTVIEW_IDENTIFIER_PARTIAL+"remove-button");
                removeButton.clickable = null;
                removeButton.clicked += RemoveClicked;
                Button addButton = this.Q<Button>(LISTVIEW_IDENTIFIER_PARTIAL+"add-button");
                addButton.clickable = null;
                addButton.clicked += AddClicked;
            });
        }

        /// <summary>
        /// Method for creating a new item in the list view.
        /// </summary>
        /// <returns>The created item as a VisualElement.</returns>
        private VisualElement MakeItem() {
            VisualElement rowItem = new VisualElement();
            if (reorderable) {
                // Add handlebars with hand symbol to imply dragging support
                rowItem.Add(HandleBarsPartial.Instantiate()[0]);
            }
            rowItem.AddToClassList(itemRowContainer);
            createRow?.Invoke(rowItem);
            return rowItem;
        }

        /// <summary>
        /// Method for handling the add button click event.
        /// </summary>
        protected virtual void AddClicked() {
            addClicked?.Invoke();
            ClearSelection();
        }

        /// <summary>
        /// Method for handling the remove button click event.
        /// </summary>
        protected virtual void RemoveClicked() {
            removeClicked?.Invoke();
            ClearSelection();
        }
    }
}
