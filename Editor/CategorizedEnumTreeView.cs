using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using System.Linq;
using UnityEngine;
using System;
using UnityEditor;

namespace CategorizedEnum {
    public class CategorizedEnumTreeView : TreeView {

        /// <summary>
        /// Additional data tracked with every treeviewitem
        /// </summary>
        public class CategorizedViewItem : TreeViewItem {
            public string fullPath = "";
            public string fullPathToLower = "";
            public int originalIndex = -1;

            public CategorizedViewItem(string fullPath, int originalIndex, int id, int depth, string displayName) {
                this.fullPath = fullPath;
                this.fullPathToLower = fullPath.ToLower();
                this.originalIndex = originalIndex;
                this.id = id;
                this.depth = depth;
                this.displayName = displayName;
            }
        }

        private bool showFullPath = false;
        private List<string> enumNames;
        private List<CategorizedViewItem> enumItems = new List<CategorizedViewItem>();
        private string lastSearch = "";
        private bool stylesInitialized = false;
        private GUIStyle pathLabel = null;
        private readonly Action<int> onSelectionChanged = null;
        private readonly char delimiter;

        private void SetupStyles() {
            if (!stylesInitialized) {
                pathLabel = new GUIStyle(EditorStyles.label);
                pathLabel.alignment = TextAnchor.MiddleLeft;
                pathLabel.clipping = TextClipping.Overflow;
                pathLabel.fontSize = 11;
                pathLabel.normal.textColor = Color.white;
                pathLabel.active.textColor = Color.white;
                pathLabel.hover.textColor = Color.white;
            }
            stylesInitialized = true;
        }

        public CategorizedEnumTreeView(TreeViewState treeViewState, bool showFullPath, char delimiter, Action<int> onSelectionChanged, string[] enumNames, int currentEnumIndex, string frameItem = "") : base(treeViewState) {
            this.enumNames = enumNames.ToList();
            this.onSelectionChanged = onSelectionChanged;
            this.delimiter = delimiter;
            this.showFullPath = showFullPath;
            Reload();
            SelectByEnumIndex(currentEnumIndex);

            if (!string.IsNullOrEmpty(frameItem)) {
                FocusByName(frameItem);
            }
        }

        /// <summary>
        /// Can this item multi select?
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        protected override bool CanMultiSelect(TreeViewItem item) {
            // disable functionality
            return false;
        }

        /// <summary>
        /// Match search against fullPath
        /// </summary>
        /// <param name="item"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        protected override bool DoesItemMatchSearch(TreeViewItem item, string search) {
            bool matches = false;
            GetCategorizedViewItem(item.id, _ => matches = _.fullPathToLower.IndexOf(search.ToLower()) != -1);
            return matches;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="onSuccess"></param>
        /// <param name="checkIndex"></param>
        /// <returns></returns>
        private bool GetCategorizedViewItem(int id, Action<CategorizedViewItem> onItemFound = null) {
            // check if the given item is a fully qualified item or just a dummy category
            bool isRealItemOrDummy = enumItems[id].originalIndex >= 0;
            if (id >= 0 && id < enumItems.Count && isRealItemOrDummy) {
                onItemFound?.Invoke(enumItems[id]);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Called when the selection changes.
        /// </summary>
        /// <param name="selectedIds">List of selected view item ids</param>
        protected override void SelectionChanged(IList<int> selectedIds) {
            base.SelectionChanged(selectedIds);
            if (selectedIds != null && selectedIds.Count > 0) {
                int selectedID = selectedIds[0];
                if (selectedID >= 0 && selectedID < enumItems.Count && enumItems[selectedID].originalIndex >= 0) {
                    onSelectionChanged(enumItems[selectedID].originalIndex);
                }
            }
        }

        /// <summary>
        /// Called when the search string changed
        /// </summary>
        /// <param name="newSearch"></param>
        protected override void SearchChanged(string newSearch) {
            base.SearchChanged(newSearch);
            // detect if search was cleared
            if (string.IsNullOrEmpty(newSearch) && !string.IsNullOrEmpty(lastSearch)) {
                // collapse hierachy
                CollapseAll();
                // select & expand last highlighted value
                var selection = GetSelection();
                if (selection != null && selection.Count > 0) {
                    FrameItem(selection[0]);
                }
            }
            // update lastSearch reference
            lastSearch = searchString;
        }

        private void FocusByName(string name) {
            var selectedItem = enumItems.Find(_ => _.fullPath.ToLowerInvariant() == name.ToLowerInvariant());

            if (selectedItem != null) {
                FrameItem(selectedItem.id);
                state.scrollPos.y -= 50;
            }
        }

        /// <summary>
        /// Set the selection by an original enum index
        /// </summary>
        /// <param name="enumIndex"></param>
        private void SelectByEnumIndex(int enumIndex) {
            // set the selection given the enum index
            var selectedItem = enumItems.Find(_ => _.originalIndex == enumIndex);
            if (selectedItem != null) {
                SetSelection(new List<int>() { selectedItem.id }, TreeViewSelectionOptions.RevealAndFrame);
            }
        }

        public void SelectByIndex(int enumIndex) {
            SelectByEnumIndex(enumIndex);
        }
        
        private void DrawMouseOverRowLabel(RowGUIArgs args, string content) {
            GUIStyle gStyle = new GUIStyle(pathLabel);
            gStyle.padding = new RectOffset((int)GetContentIndent(args.item), gStyle.padding.right, gStyle.padding.top, gStyle.padding.bottom);
            EditorGUI.LabelField(args.rowRect, content, gStyle);
        }

        protected override void RowGUI(RowGUIArgs args) {
            // if the user is not searching we draw the hierachy with indendation and foldouts for readability
            if (string.IsNullOrWhiteSpace(searchString)) {
                bool drawCustom = true;

                if (showFullPath) {
                    // if the mouse is over a fully qualified view item, show the full path
                    if (drawCustom = args.rowRect.Contains(Event.current.mousePosition)) {
                        drawCustom = GetCategorizedViewItem(args.item.id, (categorizedViewItem) => {
                            DrawMouseOverRowLabel(args, categorizedViewItem.fullPath);
                        });
                    }
                } else {
                    drawCustom = false;
                }

                // otherwise draw the default GUI
                if (!drawCustom) {
                    base.RowGUI(args);
                }
            } else {
                // if the user is searching, we display the full path instead of a hierachy & we only show fully qualified entries
                GetCategorizedViewItem(args.item.id, (categorizedViewItem) => {
                    EditorGUI.LabelField(args.rowRect, categorizedViewItem.fullPath);
                });
            }
        }

        public override void OnGUI(Rect rect) {
            SetupStyles();
            base.OnGUI(rect);
        }

        /// <summary>
        /// Extract all view items from the given enumNames
        /// </summary>
        private void ExtractViewItems() {
            int id = 0;
            enumItems.Clear();

            for (int i = 0; i < enumNames.Count; i++) {
                string[] splits = enumNames[i].Split(delimiter);

                // extract parents
                if (splits.Length > 1) {
                    string path = splits[0];
                    for (int j = 0; j < splits.Length - 1; j++) {
                        string name = splits[j];
                        if (j > 0) {
                            path += delimiter + name;
                        }
                        if (!enumItems.Exists(_ => _.fullPath == path)) {
                            // add with dummy index
                            enumItems.Add(new CategorizedViewItem(path, -1, id, j, name));
                            id++;
                        }
                    }
                }
                
                // extract fully qualified item (with original enum index)
                int lastIndex = splits.Length - 1;
                enumItems.Add(new CategorizedViewItem(enumNames[i], i, id, lastIndex, splits[lastIndex]));
                id++;
            }
        }

        protected override TreeViewItem BuildRoot() {
            // BuildRoot is called every time Reload is called to ensure that TreeViewItems 
            // are created from data. Here we just create a fixed set of items, in a real world example
            // a data model should be passed into the TreeView and the items created from the model.

            // This section illustrates that IDs should be unique and that the root item is required to 
            // have a depth of -1 and the rest of the items increment from that.
            var root = new TreeViewItem { id = -1, depth = -1, displayName = "Root" };

            // Extract all view items
            ExtractViewItems();

            // Utility method that initializes the TreeViewItem.children and -parent for all items.
            SetupParentsAndChildrenFromDepths(root, enumItems.OrderBy(_ => _.fullPathToLower).Cast<TreeViewItem>().ToList());
            
            // Return root of the tree
            return root;
        }
    }
}

