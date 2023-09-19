using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public static class UIElementsHelper {
    /// <summary>
    /// Create a PropertyField with a callback.
    /// </summary>
    /// <param name="property">SerializedProperty that should be used to create the PropertyField.</param>
    /// <param name="changeCallback">Callback that gets triggered if a value changed.</param>
    /// <returns></returns>
    public static PropertyField CreatePropertyFieldWithCallback(SerializedProperty property, EventCallback<SerializedPropertyChangeEvent> changeCallback = null) {
        PropertyField propertyField = new PropertyField(property);
        if (changeCallback != null) {
            propertyField.RegisterCallback(changeCallback);
        }
        return propertyField;
    }

    /// <summary>
    /// This is an alternative to InspectorElement.FillDefaultInspector that works without having to provide an editor object.
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <param name="root">The UI root object.</param>
    /// <param name="changeCallback">Optional: Callback fired when a property changed.</param>
    /// <param name="CreateAdditionalUI">Optional: A method executed to create additional UI for every row.</param>
    /// <param name="rootPropertyPath">Optional: A relative property path that can be provided to start with a different property inside the serializedObject.</param>
    public static void CreateGenericUI(SerializedObject serializedObject, VisualElement root, EventCallback<SerializedPropertyChangeEvent> changeCallback = null, System.Action<SerializedProperty, VisualElement> CreateAdditionalUI = null, string rootPropertyPath = null) {

        SerializedProperty rootProperty = null;
        if (rootPropertyPath != null) {
            rootProperty = serializedObject.FindProperty(rootPropertyPath);
        }

        Action<SerializedProperty, ScrollView> creationLogic;
        if (CreateAdditionalUI != null) {
            creationLogic = (prop, scrollView) => {
                VisualElement container = new VisualElement();
                container.AddToClassList(nameof(container) + nameof(PropertyField));
                container.Add(CreatePropertyFieldWithCallback(prop, changeCallback));
                CreateAdditionalUI(prop, container);
                scrollView.Add(container);
            };
        } else {
            creationLogic = (prop, scrollView) => {
                scrollView.Add(CreatePropertyFieldWithCallback(prop, changeCallback));
            };
        }

        void ForEachProperty(ref ScrollView scrollView) {
            SerializedProperty prop = rootProperty == null ? serializedObject.GetIterator() : rootProperty;
            if (prop.NextVisible(true)) {
                do {
                    if (prop.name != "m_Script") {
                        creationLogic(prop.Copy(), scrollView);
                    }
                }
                while (prop.NextVisible(false));
            }
        }

        ScrollView scrollView = new ScrollView();
        scrollView.AddToClassList("propertyList" + nameof(ScrollView));
        root.Add(scrollView);
        ForEachProperty(ref scrollView);

    }

    /// <summary>
    /// Create Generic UI for Visual Element -> https://forum.unity.com/threads/ui-elements-draw-default-inspector.773087/
    /// </summary>
    /// <param name="serializedObject"></param>
    /// <param name="root">The UI root object.</param>
    public static void CreateGenericUI(SerializedObject serializedObject, VisualElement root) {
        SerializedProperty iterator = serializedObject.GetIterator();
        Type targetType = serializedObject.targetObject.GetType();
        List<MemberInfo> members = new List<MemberInfo>(targetType.GetMembers());

        if (!iterator.NextVisible(true))
            return;
        do {
            PropertyField propertyField = new PropertyField(iterator.Copy()) {
                name = "PropertyField:" + iterator.propertyPath
            };

            MemberInfo member = members.Find(x => x.Name == propertyField.bindingPath);
            if (member != null) {
                IEnumerable<Attribute> headers = member.GetCustomAttributes(typeof(HeaderAttribute));
                IEnumerable<Attribute> spaces = member.GetCustomAttributes(typeof(SpaceAttribute));

                foreach (Attribute x in headers) {
                    HeaderAttribute actual = (HeaderAttribute)x;
                    Label header = new Label { text = actual.header };
                    header.style.unityFontStyleAndWeight = FontStyle.Bold;
                    root.Add(new Label { text = " ", name = "Header Spacer" });
                    root.Add(header);
                }
                foreach (Attribute unused in spaces) {
                    root.Add(new Label { text = " " });
                }
            }

            if (iterator.propertyPath == "m_Script" && serializedObject.targetObject != null) {
                propertyField.SetEnabled(value: false);
            }

            root.Add(propertyField);
        }
        while (iterator.NextVisible(false));
    }

    /// <summary>
    /// Creates a Vertical Spacer
    /// </summary>
    /// <param name="space">Space in pixel</param>
    /// <returns>Spacer as VisualElement</returns>
    public static VisualElement CreateVerticalSpacer(float space) {
        VisualElement spacer = new VisualElement();
        spacer.style.height = space;
        return spacer;
    }

    /// <summary>
    /// Creates a Horizontal Spacer
    /// </summary>
    /// <param name="space">Space in pixel</param>
    /// <returns>Spacer as VisualElement</returns>
    public static VisualElement CreateHorizontalSpacer(float space) {
        VisualElement spacer = new VisualElement();
        spacer.style.width = space;
        return spacer;
    }
}

