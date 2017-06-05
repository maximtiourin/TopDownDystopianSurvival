using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

namespace Fizzik {
    public class DeveloperWindow : FizzikSubWindow {
        const string defaultTitle = "Developer";

        const bool RESIZABLE = true;

        const int MOUSE_DRAG_BUTTON = 0;
        
        private Vector2 scrollPosition;
        private string output = "";

        public DeveloperWindow(FizzikSpriteEditor editor) : base(editor) {

        }

        public override void handleGUI(int windowID) {
            base.handleGUI(windowID);

            Event e = Event.current;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUIStyle outputStyle = new GUIStyle(GUI.skin.label);
            outputStyle.wordWrap = true;

            GUILayout.Label(output, outputStyle);

            GUILayout.EndScrollView();


            handleCursors();

            dragWindow();
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
        public void append(object str) {
            output += str.ToString();
        }

        /*
         * Input = "val1"
         * OutputString += "val1\n"
         */
        public void appendLine(object str) {
            output += str.ToString() + "\n";
        }

        /*
         * Input = "val1", "val2", "val3"
         * OutputString += "val1, val2, val3"
         *
         * Accepts any value type, but will attempt to convert it to a string calling its toString() method
         */
        public void appendCSV(params object[] vals) {
            string str = "";
            for (int i = 0; i < vals.Length; i++) {
                if (i < vals.Length - 1) {
                    str += vals[i].ToString() + ", ";
                }
                else {
                    str += vals[i].ToString();
                }
            }
            output += str;
        }

        public string getOutput() {
            return output;
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
            return dss_DeveloperWindow_rect;
        }

        public override Rect getMinRect() {
            return dss_DeveloperWindow_minrect;
        }

        public override string getSubWindowStringIdentifier() {
            return txt_editorprefs_identifier;
        }

        public override GUIStyle getGUIStyle(GUISkin skin) {
            GUIStyle guiStyle = new GUIStyle(skin.window);

            //Fixed size
            //guiStyle.fixedWidth = dss_DeveloperWindow_rect.size.x;
            //guiStyle.fixedHeight = dss_DeveloperWindow_rect.size.y;

            return guiStyle;
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_DeveloperWindow_rect = new Rect(0, 0, 400, 200);
        public static Rect dss_DeveloperWindow_minrect = new Rect(0, 0, 100, 40);

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_identifier = "developerWindow";
    }
}
