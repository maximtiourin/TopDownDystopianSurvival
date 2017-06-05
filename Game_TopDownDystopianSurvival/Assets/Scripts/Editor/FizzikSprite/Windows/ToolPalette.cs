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
            base.handleGUI(windowID);

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

        public override string getSubWindowStringIdentifier() {
            return txt_editorprefs_identifier;
        }

        public override GUIStyle getGUIStyle(GUISkin skin) {
            GUIStyle guiStyle = new GUIStyle(skin.window);

            //Fixed size
            guiStyle.fixedWidth = dss_ToolPalette_rect.size.x;
            guiStyle.fixedHeight = dss_ToolPalette_rect.size.y;

            return guiStyle;
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_ToolPalette_rect = new Rect(0, 0, 100, 400);

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_identifier = "toolPalette";
    }
}
