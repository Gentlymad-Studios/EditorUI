using UnityEngine;
using UnityEditor;
using CategorizedEnum;

[CustomPropertyDrawer(typeof(LocalizationPreviewAttribute))]
public class LocalizationPreviewDrawer : PropertyDrawer {

    private int idHash;
    private string selectedControlID = "";
    public static int selectedEnumIndex = -1;

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var dummyLocaAttribute = (LocalizationPreviewAttribute)attribute;

        selectedControlID = nameof(LocalizationPreviewAttribute) + "." + idHash.ToString();
        GUI.SetNextControlName(selectedControlID);
        CategorizedEnumDrawer.DrawCategorizedEnum(ref idHash, dummyLocaAttribute.showFullPath, dummyLocaAttribute.delimiter, position, property, label);

        if (GUI.GetNameOfFocusedControl() == selectedControlID) {
            selectedEnumIndex = property.intValue;
        }

    }
}
