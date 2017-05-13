using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    /*
     * A tool palette is a utility-esque class that contains the styling function for a ToolPalette to be used in the FizzikSpriteEditor
     */
    public class ToolPalette : FizzikSubWindow {
        public static string defaultTitle = "Tool Palette";

        private FizzikSpriteEditor editor;
        private Vector2 relativePos = Vector2.zero;
        private Rect currentRect;
        private int windowID;
        private bool enabled = true;

        public ToolPalette(FizzikSpriteEditor editor) {
            this.editor = editor;

            loadUserSettings();
        }

        public void handleGUI(int windowID) {
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Toggle Grid")) {
                editor.toggleGridOverlay();
            }

            if (GUILayout.Button("Reset Zoom")) {
                editor.resetCanvasZoomArea();
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
            guiStyle.fixedWidth = dss_ToolPalette_rect.size.x;
            guiStyle.fixedHeight = dss_ToolPalette_rect.size.y;

            return guiStyle;
        }

        public void loadUserSettings() {
            currentRect = new Rect(
                EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_ToolPalette_rect.x),
                EditorPrefs.GetFloat(txt_editorprefs_recty, dss_ToolPalette_rect.y),
                EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_ToolPalette_rect.size.x),
                EditorPrefs.GetFloat(txt_editorprefs_recth, dss_ToolPalette_rect.size.y)
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
        public static Rect dss_ToolPalette_rect = new Rect(0, 0, 100, 400);

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_rectx = "Fizzik.toolPalette_rectx";
        const string txt_editorprefs_recty = "Fizzik.toolPalette_recty";
        const string txt_editorprefs_rectw = "Fizzik.toolPalette_rectw";
        const string txt_editorprefs_recth = "Fizzik.toolPalette_recth";
        const string txt_editorprefs_enabled = "Fizzik.toolPalette_enabled";
    }
}
