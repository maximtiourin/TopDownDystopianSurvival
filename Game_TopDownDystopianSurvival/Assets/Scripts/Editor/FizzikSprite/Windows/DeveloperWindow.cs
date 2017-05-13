using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class DeveloperWindow : FizzikSubWindow {
        public static string defaultTitle = "Developer";

        private FizzikSpriteEditor editor;
        private Vector2 relativePos = Vector2.zero;
        private Rect currentRect;
        private int windowID;
        private bool enabled = false;
        
        private Vector2 scrollPosition;
        private string output = "dlshfkjsdhfkjsdhf\naskjdhaskjdhas\najskdhajkshdkjashdkjas\njkahsdkjahsd\nasdjhaskdjhasddlshfkjsdhfkjsdhf\naskjdhaskjdhas\najskdhajkshdkjashdkjas\njkahsdkjahsd\nasdjhaskdjhasddlshfkjsdhfkjsdhf\naskjdhaskjdhas\najskdhajkshdkjashdkjas\njkahsdkjahsd\nasdjhaskdjhasddlshfkjsdhfkjsdhf\naskjdhaskjdhas\najskdhajkshdkjashdkjas\njkahsdkjahsd\nasdjhaskdjhasddlshfkjsdhfkjsdhf\naskjdhaskjdhas\najskdhajkshdkjashdkjas\njkahsdkjahsd\nasdjhaskdjhasddlshfkjsdhfkjsdhf\naskjdhaskjdhas\najskdhajkshdkjashdkjas\njkahsdkjahsd\nasdjhaskdjhasd";

        public DeveloperWindow(FizzikSpriteEditor editor) {
            this.editor = editor;

            loadUserSettings();
        }

        public void handleGUI(int windowID) {
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUIStyle outputStyle = new GUIStyle(GUI.skin.label);
            outputStyle.wordWrap = true;

            GUILayout.Label(output, outputStyle);

            GUILayout.EndScrollView();

            GUI.DragWindow();
        }

        /*
         * Clears output string
         */
        public void clear() {
            output = "";
        }

        /*
         * Input = "val1"
         * OutputString += "val1"
         */
        public void append(string str) {
            output += str;
        }

        /*
         * Input = "val1"
         * OutputString += "val1\n"
         */
        public void appendLine(string str) {
            output += str + "\n";
        }

        /*
         * Input = "val1", "val2", "val3"
         * OutputString += "val1, val2, val3"
         */
        public void appendCSV(params string[] vals) {
            string str = "";
            for (int i = 0; i < vals.Length; i++) {
                if (i < vals.Length - 1) {
                    str += vals[i] + ", ";
                }
                else {
                    str += vals[i];
                }
            }
            output += str;
        }

        public string getOutput() {
            return output;
        }

        /*
         * Restricts this currentRect to fall inside of the other rect
         */
        public void clampInsideRect(Rect other) {
            float w = currentRect.size.x;
            float h = currentRect.size.y;

            currentRect.x = Mathf.Max(other.x, Mathf.Min(currentRect.x, other.x + other.size.x - w));
            currentRect.y = Mathf.Max(other.y, Mathf.Min(currentRect.y, other.y + other.size.y - h));

            //Fixed size
            //currentRect.size = new Vector2(w, h);
        }

        public Rect getCurrentRect() {
            return currentRect;
        }

        public void setCurrentRect(Rect rect) {
            currentRect = rect;
        }

        public int getWindowID() {
            return windowID;
        }

        public void setWindowID(int windowID) {
            this.windowID = windowID;
        }

        public string getTitle() {
            return defaultTitle;
        }

        public GUIStyle getGUIStyle(GUISkin skin) {
            GUIStyle guiStyle = new GUIStyle(skin.window);

            //Fixed size
            guiStyle.fixedWidth = dss_DeveloperWindow_rect.size.x;
            guiStyle.fixedHeight = dss_DeveloperWindow_rect.size.y;

            return guiStyle;
        }

        public void loadUserSettings() {
            currentRect = new Rect(
                EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_DeveloperWindow_rect.x),
                EditorPrefs.GetFloat(txt_editorprefs_recty, dss_DeveloperWindow_rect.y),
                EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_DeveloperWindow_rect.size.x),
                EditorPrefs.GetFloat(txt_editorprefs_recth, dss_DeveloperWindow_rect.size.y)
            );
            enabled = EditorPrefs.GetBool(txt_editorprefs_enabled, enabled);
        }

        public void saveUserSettings() {
            EditorPrefs.SetFloat(txt_editorprefs_rectx, currentRect.x);
            EditorPrefs.SetFloat(txt_editorprefs_recty, currentRect.y);
            EditorPrefs.SetFloat(txt_editorprefs_rectw, currentRect.size.x);
            EditorPrefs.SetFloat(txt_editorprefs_recth, currentRect.size.y);
            EditorPrefs.SetBool(txt_editorprefs_enabled, enabled);
        }

        public void toggleEnabled() {
            enabled = !enabled;
        }

        public bool isEnabled() {
            return enabled;
        }

        public Vector2 getRelativeWindowPosition() {
            return relativePos;
        }

        public void setRelativeWindowPosition(Vector2 relpos) {
            relativePos = relpos;
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_DeveloperWindow_rect = new Rect(0, 0, 400, 200);

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_rectx = "Fizzik.developerWindow_rectx";
        const string txt_editorprefs_recty = "Fizzik.developerWindow_recty";
        const string txt_editorprefs_rectw = "Fizzik.developerWindow_rectw";
        const string txt_editorprefs_recth = "Fizzik.developerWindow_recth";
        const string txt_editorprefs_enabled = "Fizzik.developerWindow_enabled";
    }
}
