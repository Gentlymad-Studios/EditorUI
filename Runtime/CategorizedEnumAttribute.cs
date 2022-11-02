using System;
using UnityEngine;

namespace CategorizedEnum {
    [AttributeUsage(AttributeTargets.Field)]
    public class CategorizedEnumAttribute : PropertyAttribute {
        /// <summary>
        /// The delimiter used to categorize the enum entries
        /// </summary>
        public readonly char delimiter = '_';

        /// <summary>
        /// The referenced enum type
        /// </summary>
        public readonly System.Type enumType = null;
        
        /// <summary>
        /// The referenced enum type
        /// </summary>
        public readonly bool displayInViewer = false;

        /// <summary>
        /// Show id on hover
        /// </summary>
        public readonly bool showFullPath = false;

        /// <summary>
        /// If we should categorize enum entries by camel case
        /// </summary>
        public bool categorizeByCamelCase = false;

        /// <summary>
        /// When the underlying property is simply an int instead of an enum, we notify the drawer that this is a FakeEnum
        /// </summary>
        public bool isFakeEnum = false;

        public CategorizedEnumAttribute(char delimiter = '_', System.Type enumType = null, bool displayInViewer = false, bool showID = false, bool isFakeEnum = false) {
            this.delimiter = delimiter;
            this.enumType = enumType;
            this.displayInViewer = displayInViewer;
            this.showFullPath = showID;
            this.isFakeEnum = isFakeEnum;
        }

    }
}


