using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    /*
     * A tool palette is a utility-esque class that contains the styling function for a ToolPalette to be used in the FizzikSpriteEditor
     */
    public class ToolPalette : FizzikSubWindow {
        const string defaultTitle = "Tool Palette";

        const bool RESIZABLE = false;

        const int MOUSE_DRAG_BUTTON = 0;

        public ToolPalette(FizzikSpriteEditor editor) : base(editor) {

        }

        public override void handleGUI(int windowID) {
            EditorGUILayout.BeginVertical();

            if (GUILayout.Button("Toggle Grid")) {
                editor.toggleGridOverlay();
            }

            if (GUILayout.Button("Reset Zoom")) {
                editor.resetCanvasZoomArea();
            }

            EditorGUILayout.EndVertical();

            dragWindow();
        }

        public override string getTitle() {
            return defaultTitle;
        }

        public override int getMouseDragButton() {
            return MOUSE_DRAG_BUTTON;
        }

        public override bool isResizable() {
            return RESIZABLE;
        }

        public override Rect getDefaultRect() {
            return dss_ToolPalette_rect;
        }

        public override GUIStyle getGUIStyle(GUISkin skin) {
            GUIStyle guiStyle = new GUIStyle(skin.window);

            //Fixed size
            guiStyle.fixedWidth = dss_ToolPalette_rect.size.x;
            guiStyle.fixedHeight = dss_ToolPalette_rect.size.y;

            return guiStyle;
        }

        public override void loadUserSettings() {
            if (isResizable()) {
                setCurrentRect(new Rect(
                    EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_ToolPalette_rect.x),
                    EditorPrefs.GetFloat(txt_editorprefs_recty, dss_ToolPalette_rect.y),
                    EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_ToolPalette_rect.size.x),
                    EditorPrefs.GetFloat(txt_editorprefs_recth, dss_ToolPalette_rect.size.y)
                ));
            }
            else {
                setCurrentRect(new Rect(
                    EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_ToolPalette_rect.x),
                    EditorPrefs.GetFloat(txt_editorprefs_recty, dss_ToolPalette_rect.y),
                    dss_ToolPalette_rect.size.x, //Fixed default size
                    dss_ToolPalette_rect.size.y //Fixed default size
                ));
            }
            enabled = EditorPrefs.GetBool(txt_editorprefs_enabled, enabled);
        }

        public override void saveUserSettings() {
            EditorPrefs.SetFloat(txt_editorprefs_rectx, currentRect.x);
            EditorPrefs.SetFloat(txt_editorprefs_recty, currentRect.y);
            EditorPrefs.SetFloat(txt_editorprefs_rectw, currentRect.size.x);
            EditorPrefs.SetFloat(txt_editorprefs_recth, currentRect.size.y);
            EditorPrefs.SetBool(txt_editorprefs_enabled, enabled);
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
