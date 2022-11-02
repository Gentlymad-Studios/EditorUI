using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace CategorizedEnum {

    public class CategorizedEnumDrawer<T> where T : System.Enum {
        private char delimiter = '_';
        private bool showFullPath = false;
        private string[] enumNames;
        private int[] enumValues;
        private int idHash;
        private int enumIndex;

        private float customWidth = -1;
        private GUIContent label = null;
        private GUIContent enumDisplayNameLabel = null;

        public CategorizedEnumDrawer(bool showFullPath = false, char delimiter = '_', float customWidth = -1, string labelContent = null) {
            GetIDHash();
            enumNames = Enum.GetNames(typeof(T));
            enumValues = Enum.GetValues(typeof(T)).Cast<int>().ToArray();
            this.delimiter = delimiter;
            this.customWidth = customWidth;
            this.showFullPath = showFullPath;
        }

        private void GetIDHash() {
            if (idHash == 0) {
                idHash = typeof(T).ToString().GetHashCode();
            }
        }

        private void OnSelect(int index) {

        }

        public void DrawCategorizedEnum(int enumValueIndex, Rect position) {

            enumIndex = -1;
            for (int i = 0; i < enumNames.Length; ++i) {
                if (enumValues[i] == enumValueIndex) {
                    enumIndex = i;
                    break;
                }
            }
            if (enumIndex >= enumNames.Length || enumIndex < 0) {
                enumIndex = Mathf.Abs(enumIndex % enumNames.Length);
            }

            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            position = EditorGUI.PrefixLabel(position, id, label);

            enumDisplayNameLabel.text = enumNames[enumIndex];
            if (CategorizedEnumDrawer.DropdownButton(id, position, enumDisplayNameLabel)) {
                if (customWidth > 0) {
                    position.width = customWidth;
                }

                CategorizedEnumPopup.Show(position, delimiter, showFullPath, enumNames, enumIndex, OnSelect);
            }
        }
    }

    public class CategorizedStringDrawer {
        private char delimiter = '_';
        private bool showFullPath = false;
        private string[] values;

        private int idHash;
        private int index;

        private float customWidth = -1;
        private GUIContent label = null;
        private GUIContent displayNameLabel = null;

        public string currentValue;

        public CategorizedStringDrawer(string id, string value, string[] values, bool showFullPath = false, char delimiter = '_', float customWidth = -1) {
            this.values = values;
            this.idHash = id.GetHashCode();
            this.delimiter = delimiter;
            this.customWidth = customWidth;
            this.currentValue = value;
            this.showFullPath = showFullPath;
        }

        public CategorizedStringDrawer(string id, bool showFullPath = false, char delimiter = '_', float customWidth = -1) {
            this.idHash = id.GetHashCode();
            this.delimiter = delimiter;
            this.customWidth = customWidth;
            this.showFullPath = showFullPath;
        }

        public void Update(string[] values, string selectedValue) {
            this.values = values;

            for (int i = 0; i < values.Length; i++) {
                if (selectedValue == values[i]) {
                    this.currentValue = selectedValue;
                    this.index = i;
                    return;
                }
            }

            this.currentValue = "";
            this.index = 0;
        }

        private void OnSelect(int index) {
            this.index = index;
            this.currentValue = values[index];
        }

        public void Draw(Rect position) {
            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            position = EditorGUI.PrefixLabel(position, id, label);

            if (label == null) {
                label = new GUIContent();
                displayNameLabel = new GUIContent();
            }

            if (values != null) {
                displayNameLabel.text = values[index];
                if (CategorizedEnumDrawer.DropdownButton(id, position, displayNameLabel)) {
                    if (customWidth > 0) {
                        position.width = customWidth;
                    }

                    CategorizedEnumPopup.Show(position, delimiter, showFullPath, values, index, OnSelect);
                }
            }
        }
    }

    /// <summary>
    /// Draws the custom enum selector popup for enum fileds using the
    /// SearchableEnumAttribute.
    /// </summary>
    [CustomPropertyDrawer(typeof(CategorizedEnumAttribute))]
    public class CategorizedEnumDrawer : PropertyDrawer {

        /// <summary>
        /// Cache of the hash to use to resolve the ID for the drawer.
        /// </summary>
        private int idHash;

        /// <summary>
        /// Reference to the attribute
        /// </summary>
        private CategorizedEnumAttribute categorizedEnumAttribute = null;

        private static Dictionary<Type, string[]> enumNamesLUT = new Dictionary<Type, string[]>();
        private static Dictionary<Type, Dictionary<int,int>> enumValuesToIndexLUT = new Dictionary<Type, Dictionary<int, int>>();
        private static Dictionary<Type, int[]> enumValuesLUT = new Dictionary<Type, int[]>();
        private static GUIContent buttonText = null;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            if (categorizedEnumAttribute == null) {
                categorizedEnumAttribute = attribute as CategorizedEnumAttribute;
            }

            if (categorizedEnumAttribute.displayInViewer && EnumViewer.viewerAvailable) {
                DrawCategorizedEnumUsingTypeInViewer(ref idHash, categorizedEnumAttribute.enumType, categorizedEnumAttribute.showFullPath, categorizedEnumAttribute.delimiter, position, property, label);
            } else {
                if (categorizedEnumAttribute.enumType != null) {
                    if (categorizedEnumAttribute.isFakeEnum) {
                        DrawCategorizedFakeEnum(ref idHash, categorizedEnumAttribute.enumType, categorizedEnumAttribute.showFullPath, categorizedEnumAttribute.delimiter, position, property, label);
                    } else {
                        DrawCategorizedEnumUsingType(ref idHash, categorizedEnumAttribute.enumType, categorizedEnumAttribute.showFullPath, categorizedEnumAttribute.delimiter, position, property, label);
                    }
                } else {
                    DrawCategorizedEnum(ref idHash, categorizedEnumAttribute.showFullPath, categorizedEnumAttribute.delimiter, position, property, label);
                }
            }
        }

        private static Type GetType(SerializedProperty prop) {
            //gets parent type info
            string[] slices = prop.propertyPath.Split('.');
            Type type = prop.serializedObject.targetObject.GetType();

            for (int i = 0; i < slices.Length; i++)
                if (slices[i] == "Array") {
                    i++; //skips "data[x]"
                    type = type.GetElementType(); //gets info on array elements
                }
                //gets info on field and its type
                else type = type.GetField(slices[i], BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.FlattenHierarchy | BindingFlags.Instance).FieldType;
            return type;
        }
        
        public static void DrawCategorizedEnumUsingTypeInViewer(ref int idHash, Type enumType, bool showFullPath, char delimiter, Rect position, SerializedProperty property, GUIContent label) {
            DrawCategorizedEnumWithViewer(ref idHash, enumType, showFullPath, delimiter, position, property, label);
        }

        public static void DrawCategorizedEnumUsingType(ref int idHash, Type enumType, bool showFullPath, char delimiter, Rect position, SerializedProperty property, GUIContent label) {
            if (!enumNamesLUT.ContainsKey(enumType)) {
                enumNamesLUT.Add(enumType, Enum.GetNames(enumType));
            }
            DrawCategorizedEnum(ref idHash, enumNamesLUT[enumType], delimiter, showFullPath, position, property, label);
        }

        public static void DrawCategorizedFakeEnum(ref int idHash, Type enumType, bool showFullPath, char delimiter, Rect position, SerializedProperty property, GUIContent label) {
            if (!enumNamesLUT.ContainsKey(enumType)) {
                enumNamesLUT.Add(enumType, Enum.GetNames(enumType));
            }
            if (!enumValuesToIndexLUT.ContainsKey(enumType)) {
                int[] values = Enum.GetValues(enumType).Cast<int>().ToArray();
                if (!enumValuesLUT.ContainsKey(enumType)) {
                    enumValuesLUT.Add(enumType, values);
                } else {
                    enumValuesLUT[enumType] = values;
                }
                Dictionary<int, int> valuetoIndexMapper = new Dictionary<int, int>();
                for (int i=0; i< values.Length; i++) {
                    valuetoIndexMapper.Add(values[i], i);
                }
                enumValuesToIndexLUT.Add(enumType, valuetoIndexMapper);
            }
            DrawCategorizedFakeEnum(ref idHash, enumNamesLUT[enumType], enumValuesToIndexLUT[enumType], enumValuesLUT[enumType], delimiter, showFullPath, position, property, label);
        }

        public static void DrawCategorizedEnum(ref int idHash, bool showFullPath, char delimiter, Rect position, SerializedProperty property, GUIContent label) {
            DrawCategorizedEnum(ref idHash, property.enumNames, delimiter, showFullPath, position, property, label);
        }

        public static void DrawCategorizedEnumWithPropCheck(ref int idHash, bool showFullPath, char delimiter, Rect position, SerializedProperty property, GUIContent label) {
            if(PropertyCheckEnum(position, property)) {
                DrawCategorizedEnum(ref idHash, property.enumNames, delimiter, showFullPath, position, property, label);
            }
        }

        public static bool PropertyCheckEnum(Rect position, SerializedProperty property) {
            // If this is not used on an enum, show an error
            if (property.type != "Enum") {
                GUIStyle errorStyle = "CN EntryErrorIconSmall";
                Rect r = new Rect(position);
                r.width = errorStyle.fixedWidth;
                position.xMin = r.xMax;
                GUI.Label(r, "", errorStyle);
                GUI.Label(position, nameof(CategorizedEnumDrawer) + " can only be used on enum fields.");
                return false;
            }
            return true;
        }

        private static void DrawCategorizedEnum(ref int idHash, string[] enumNames, char delimiter, bool showFullPath, Rect position, SerializedProperty property, GUIContent label) {
            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            if (idHash == 0) idHash = nameof(CategorizedEnumDrawer).GetHashCode();
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, id, label);

            //property.intValue
            int enumValueIndex = property.enumValueIndex;

            //Avoid error from negative Index
            int index = enumValueIndex;
            if (index >= enumNames.Length || index < 0) {
                index = Mathf.Abs(index % enumNames.Length);
            }

            if (buttonText == null) {
                buttonText = new GUIContent("");
            }
            buttonText.text = enumNames[index];

            UnityEngine.Object obj = property.serializedObject.targetObject;
            string pPath = property.propertyPath;

            if (DropdownButton(id, position, buttonText)) {
                Action<int> onSelect = i => {
                    SerializedObject serObj = new SerializedObject(obj);
                    SerializedProperty prop = serObj.FindProperty(pPath);
                    prop.enumValueIndex = i;
                    prop.serializedObject.ApplyModifiedProperties();
                    Events.FireOnSelect();
                };

                CategorizedEnumPopup.Show(position, delimiter, showFullPath, enumNames, enumValueIndex, onSelect);
            }
            EditorGUI.EndProperty();
        }

        private static void DrawCategorizedFakeEnum(ref int idHash, string[] enumNames, Dictionary<int,int> valuesToIndex, int[] values, char delimiter, bool showFullPath, Rect position, SerializedProperty property, GUIContent label) {
            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            if (idHash == 0) idHash = nameof(CategorizedEnumDrawer).GetHashCode();
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, id, label);

            //property.intValue
            int enumValueIndex = 0;
            if (valuesToIndex.ContainsKey(property.intValue)) {
                enumValueIndex = valuesToIndex[property.intValue];
            } else {
                Debug.LogError($"Key { property.intValue } not found for property { property.name }. Setting to 0 ...");
            }
            
            

            //Avoid error from negative Index
            int index = enumValueIndex;
            if (index >= enumNames.Length || index < 0) {
                index = Mathf.Abs(index % enumNames.Length);
            }

            if (buttonText == null) {
                buttonText = new GUIContent("");
            }
            buttonText.text = enumNames[index];

            UnityEngine.Object obj = property.serializedObject.targetObject;
            string pPath = property.propertyPath;

            if (DropdownButton(id, position, buttonText)) {
                Action<int> onSelect = i => {
                    SerializedObject serObj = new SerializedObject(obj);
                    SerializedProperty prop = serObj.FindProperty(pPath);
                    prop.intValue = values[i];
                    prop.serializedObject.ApplyModifiedProperties();
                    Events.FireOnSelect();
                };

                CategorizedEnumPopup.Show(position, delimiter, showFullPath, enumNames, enumValueIndex, onSelect);
            }
            EditorGUI.EndProperty();
        }

        private static void DrawCategorizedEnumWithViewer(ref int idHash, Type enumType, bool showFullPath, char delimiter, Rect position, SerializedProperty property, GUIContent label) {
            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            if (idHash == 0) idHash = nameof(CategorizedEnumDrawer).GetHashCode();
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            label = EditorGUI.BeginProperty(position, label, property);
            position = EditorGUI.PrefixLabel(position, id, label);

            int enumValueIndex = property.enumValueIndex;
            
            if (!enumNamesLUT.ContainsKey(enumType)) {
                enumNamesLUT.Add(enumType, Enum.GetNames(enumType));
            }
            string[] enumNames = enumNamesLUT[enumType];

            //Avoid error from negative Index
            int index = enumValueIndex;
            if (index >= enumNames.Length || index < 0) {
                index = Mathf.Abs(index % enumNames.Length);
            }

            if (buttonText == null) {
                buttonText = new GUIContent("");
            }
            buttonText.text = enumNames[index];

            UnityEngine.Object obj = property.serializedObject.targetObject;
            string pPath = property.propertyPath;

            if (DropdownButton(id, position, buttonText)) {
                Action<int> onSelectAction = (i) => {
                    SerializedObject serObj = new SerializedObject(obj);
                    SerializedProperty prop = serObj.FindProperty(pPath);
                    prop.enumValueIndex = i;
                    prop.serializedObject.ApplyModifiedProperties();
                    Events.FireOnSelect();
                };

                EnumViewer.CreateViewer(delimiter, showFullPath, enumNames, enumType, enumValueIndex);
                EnumViewer.SetupAndSelectViewer(enumType, onSelectAction, enumValueIndex, property.serializedObject.targetObject.name, property.displayName);

            }
            EditorGUI.EndProperty();
        }

        public static void DrawCategorizedEnum(ref int idHash, char delimiter, bool showFullPath, Rect position, string[] enumNames, int enumValueIndex, Action<int> onSelect, GUIContent label, float width = -1) {
            // By manually creating the control ID, we can keep the ID for the
            // label and button the same. This lets them be selected together
            // with the keyboard in the inspector, much like a normal popup.
            if (idHash == 0) idHash = nameof(CategorizedEnumDrawer).GetHashCode();
            int id = GUIUtility.GetControlID(idHash, FocusType.Keyboard, position);

            position = EditorGUI.PrefixLabel(position, id, label);

            //Avoid error from negative Index
            int index = enumValueIndex;
            if (index >= enumNames.Length || index < 0) {
                index = Mathf.Abs(index % enumNames.Length);
            }

            if (buttonText == null) {
                buttonText = new GUIContent("");
            }
            buttonText.text = enumNames[index];

            if (DropdownButton(id, position, buttonText)) {
                if (width > 0) {
                    position.width = width;
                }

                CategorizedEnumPopup.Show(position, delimiter, showFullPath, enumNames, enumValueIndex, onSelect);
            }
        }

        /// <summary>
        /// A custom button drawer that allows for a controlID so that we can
        /// sync the button ID and the label ID to allow for keyboard
        /// navigation like the built-in enum drawers.
        /// </summary>
        public static bool DropdownButton(int id, Rect position, GUIContent content) {
            Event current = Event.current;
            switch (current.type) {
                case EventType.MouseDown:
                    if (position.Contains(current.mousePosition) && current.button == 0) {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.KeyDown:
                    if (GUIUtility.keyboardControl == id && current.character == '\n') {
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.Repaint:
                    EditorStyles.popup.Draw(position, content, id, false);
                    break;
            }
            return false;
        }
    }
}