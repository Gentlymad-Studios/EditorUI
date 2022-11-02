using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(PasswordFieldAttribute))]
public class PasswordFieldDrawer : PropertyDrawer {
    private bool displayInClearText = false;
    private bool stylesInitialized = false;

    private GUIStyle labelButton;
    private GUIContent visibilityIcon;

    private void SetupStyles() {
        if (!stylesInitialized) {
            labelButton = new GUIStyle(EditorStyles.label);
            labelButton.padding = new RectOffset(2,2,2,2);
            labelButton.margin = new RectOffset();
            visibilityIcon = EditorGUIUtility.IconContent("ViewToolOrbit On");
            stylesInitialized = true;
        }
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
        var passwordFieldAttribute = (PasswordFieldAttribute)attribute;

        SetupStyles();
        position.width -= passwordFieldAttribute.toggable ? 20 : 0;
        Color defaultGUIColor = GUI.color;
        GUI.color = displayInClearText ? Color.red : defaultGUIColor;

        if (!displayInClearText) {
            EditorGUI.BeginChangeCheck();
            property.stringValue = EditorGUI.PasswordField(position, property.displayName, property.stringValue);
            if (EditorGUI.EndChangeCheck()) {
                property.serializedObject.ApplyModifiedProperties();
            }
        } else {
            EditorGUI.PropertyField(position, property);
        }

        if (passwordFieldAttribute.toggable) {
            if (GUI.Button(new Rect(position.width + position.x, position.y, 20, position.height), visibilityIcon, labelButton)) {
                displayInClearText = !displayInClearText;
            }
        }

        GUI.color = defaultGUIColor;
    }
}
