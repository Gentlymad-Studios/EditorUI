using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field, Inherited = true)]
public class LocalizationPreviewAttribute : PropertyAttribute {
    public char delimiter = '_';
    public bool showFullPath = false;
    public LocalizationPreviewAttribute(char delimiter = '_', bool showFullPath = false){
        this.delimiter = delimiter;
        this.showFullPath = showFullPath;
    }
}
