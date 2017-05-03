using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public abstract class FizzikMenuOptionsWindow : EditorWindow {
        protected FizzikSpriteEditor editor;
        protected bool shouldClose;

        public FizzikMenuOptionsWindow(FizzikSpriteEditor editor) {
            this.editor = editor;
        }

        void Update() {
            if (shouldClose) {
                Close();
            }
        }

        public void showOptions() {
            ShowUtility();
        }

        public void closeWindow() {
            shouldClose = true;
        }
    }
}
