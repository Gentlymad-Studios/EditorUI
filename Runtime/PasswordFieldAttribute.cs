using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class PasswordFieldAttribute : PropertyAttribute {

    public bool toggable = false;

    public PasswordFieldAttribute(bool toggable = false) {
        this.toggable = toggable;
    }
}
