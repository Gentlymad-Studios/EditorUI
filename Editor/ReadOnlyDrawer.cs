using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
public class ReadOnlyDrawer : PropertyDrawer {
    ReadOnlyAttribute _attribute;
    bool edit = false;
    Color tmpColor;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
        return EditorGUI.GetPropertyHeight(property, label, true);
    }

    public override void OnGUI(Rect rect, SerializedProperty prop, GUIContent label) {
        _attribute = (ReadOnlyAttribute)attribute;
        if (_attribute.showEditButton) {
            tmpColor = GUI.color;
            rect.width = rect.width - 80;
            if (edit) {
                EditorGUI.PropertyField(rect, prop);
                GUI.color = Color.yellow;
            } else {
                DrawInactive(rect, prop);
            }
            rect.x = rect.x + rect.width + 5;
            rect.width = 80 - 5;
            if(GUI.Button(rect, "Edit")) {
                edit = !edit;
            }
            GUI.color = tmpColor;
        } else {
            DrawInactive(rect, prop);
        }
    }

    private void DrawInactive(Rect rect, SerializedProperty prop) {
        bool wasEnabled = GUI.enabled;
        GUI.enabled = false;
        EditorGUI.PropertyField(rect, prop);
        GUI.enabled = wasEnabled;
    }
}

