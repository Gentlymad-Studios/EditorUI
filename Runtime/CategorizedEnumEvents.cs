#if UNITY_EDITOR
using System;
using UnityEditor;

namespace CategorizedEnum {
    public delegate void OnSelectEventEvent();
    /// <summary>
    /// This events class is used to listen to categorized enum specific events from everywhere
    /// </summary>
    public class Events {
        /// On Select event. This is useful for editor scripting if something should  update right after a enum value was changed
        public event OnSelectEventEvent onSelect;
        private static Events instance;

        /// <summary>
        /// Get the current instance of this class. Beware, this is an "Evil" Singleton.
        /// It makes sense in this case, since categorized enums don't have a central structure so having a globally referencable event class to listen to the events from anywhere is very handy.
        /// </summary>
        public static Events Instance {
            get {
                if (instance == null) {
                    Initialize();
                }
                return instance;
            }
        }

        /// <summary>
        /// Initialize method, this should initialize when the editor is opened & right after compilation
        /// </summary>
        [InitializeOnLoadMethod]
        public static void Initialize() {
            instance = new Events();
            Dispose();
            EditorApplication.wantsToQuit -= WantsToQuit;
            EditorApplication.wantsToQuit += WantsToQuit;
        }

        /// <summary>
        /// Dispose all listeners right before the editor quits
        /// </summary>
        /// <returns></returns>
        private static bool WantsToQuit() {
            Dispose();
            return true;
        }

        /// <summary>
        /// add another on select event listener
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="removePrevious"></param>
        public static void AddOnSelectListener(OnSelectEventEvent listener, bool removePrevious = true) {
            if (removePrevious) {
                instance.onSelect -= listener;
            }

            instance.onSelect += listener;
        }

        /// <summary>
        /// fire the OnSelect event
        /// </summary>
        public static void FireOnSelect() {
            instance.onSelect?.Invoke();
        }

        /// <summary>
        /// Dispose all events
        /// </summary>
        public static void Dispose() {
            // Purge OnBackendReadyEvents
            if (instance.onSelect != null) {
                foreach (Delegate d in instance.onSelect.GetInvocationList()) {
                    instance.onSelect -= (OnSelectEventEvent)d;
                }
            }
        }
    }
}
#endif