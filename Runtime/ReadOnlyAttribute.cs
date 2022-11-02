using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class ReadOnlyAttribute : PropertyAttribute {
    public bool showEditButton = false;
    public ReadOnlyAttribute(bool showEditButton = false) {
        this.showEditButton = showEditButton;
    }
}