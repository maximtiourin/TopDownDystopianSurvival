using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class ColorPalette : FizzikSubWindow {
        public static string defaultTitle = "Color Palette";

        private FizzikSpriteEditor editor;
        private Rect currentRect;
        private int windowID;

        public ColorPalette(FizzikSpriteEditor editor) {
            this.editor = editor;

            loadUserSettings();
        }

        public void handleGUI(int windowID) {
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("testbtn")) {
                EditorUtility.DisplayDialog("test", "msg", "ok");
            }

            if (GUILayout.Button("testbtn2")) {
                EditorUtility.DisplayDialog("test2", "msg2", "ok2");
            }

            EditorGUILayout.EndVertical();

            GUI.DragWindow();
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
            guiStyle.fixedWidth = dss_ColorPalette_rect.size.x;
            guiStyle.fixedHeight = dss_ColorPalette_rect.size.y;

            return guiStyle;
        }

        public void loadUserSettings() {
            currentRect = new Rect(
                EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_ColorPalette_rect.x),
                EditorPrefs.GetFloat(txt_editorprefs_recty, dss_ColorPalette_rect.y),
                EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_ColorPalette_rect.size.x),
                EditorPrefs.GetFloat(txt_editorprefs_recth, dss_ColorPalette_rect.size.y)
            );
        }

        public void saveUserSettings() {
            EditorPrefs.SetFloat(txt_editorprefs_rectx, currentRect.x);
            EditorPrefs.SetFloat(txt_editorprefs_recty, currentRect.y);
            EditorPrefs.SetFloat(txt_editorprefs_rectw, currentRect.size.x);
            EditorPrefs.SetFloat(txt_editorprefs_recth, currentRect.size.y);
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_ColorPalette_rect = new Rect(0, 0, 200, 120);

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_rectx = "colorPalette_rectx";
        const string txt_editorprefs_recty = "colorPalette_recty";
        const string txt_editorprefs_rectw = "colorPalette_rectw";
        const string txt_editorprefs_recth = "colorPalette_recth";
    }
}
