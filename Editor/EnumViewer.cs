using System;
using System.Collections.Generic;
using System.Linq;
using CategorizedEnum;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class EnumViewer {
    private static Dictionary<Type, EnumViewer> enumViewerLUT = new Dictionary<Type, EnumViewer>();
    public static EnumViewer selectedViewer = null;
    public static bool viewerAvailable = false;

    public string propertyContainerName = null;
    public string propertyName = null;
    public Type enumType = null;

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

    private bool firstRun = true;
    public bool shouldClose = false;
    private int[] enumValues = null;

    private Action<int> onSelect = null;
    private int currentIndex;

    private void OnSelect(int index) {
        shouldClose = true;
        currentIndex = index;
        onSelect?.Invoke(index);
    }

    public static void CreateViewer(char delimiter, bool showFullPath, string[] enumNames, Type enumType, int enumValueIndex) {
        if (!enumViewerLUT.ContainsKey(enumType)) {
            enumViewerLUT.Add(enumType, new EnumViewer(delimiter, showFullPath, enumNames, Enum.GetValues(enumType).Cast<int>().ToArray(), enumValueIndex));
        }
    }

    public static void SetupAndSelectViewer(Type enumType, Action<int> onSelectAction, int enumValueIndex, string propContainerName, string propName) {
        selectedViewer = enumViewerLUT[enumType];
        selectedViewer.onSelect = onSelectAction;
        selectedViewer.SelectByIndex(enumValueIndex);

        selectedViewer.propertyContainerName = "[" + propContainerName + "]";
        selectedViewer.propertyName = propName;
        selectedViewer.enumType = enumType;
    }

    public static void DeselectViewer() {
        selectedViewer = null;
    }

    public EnumViewer(char delimiter, bool showFullPath, string[] enumNames, int[] enumValues, int currentEnumIndex) {
        this.enumValues = enumValues;
        firstRun = true;

        // Check if we already had a serialized view state (state 
        // that survived assembly reloading)
        if (treeViewState == null) {
            treeViewState = new TreeViewState();
        }

        // create & setup the treeview
        currentIndex = currentEnumIndex;
        treeView = new CategorizedEnumTreeView(treeViewState, showFullPath, delimiter, OnSelect, enumNames, currentEnumIndex);

        // setup the search field
        searchField = new SearchField();
        searchField.downOrUpArrowKeyPressed += treeView.SetFocus;

    }

    public int GetValue() {
        if (currentIndex >= 0) {
            return enumValues[currentIndex];
        }
        return -1;
    }
        
    private void SelectByIndex(int enumIndex) {
        treeView.SelectByIndex(enumIndex);
        currentIndex = enumIndex;
        shouldClose = false;
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
    /// Draws the UI of the window.
    /// </summary>
    /// <param name="rect"></param>
    public void OnGUI(float minWidth=0, float maxWidth=100000, float minHeight=0, float maxHeight=100000) {
        // setup GUI styles
        SetupStyles();
        EditorGUILayout.BeginVertical(layoutGroup);
        GUI.SetNextControlName("searchString");
        treeView.searchString = searchField.OnGUI(treeView.searchString);
        EditorGUILayout.EndVertical();
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        treeView.OnGUI(GUILayoutUtility.GetRect(minWidth, maxWidth, minHeight, maxHeight));
        EditorGUILayout.EndVertical();

        if (firstRun) {
            GUI.FocusControl("searchString");
            firstRun = false;
        }
    }
}
