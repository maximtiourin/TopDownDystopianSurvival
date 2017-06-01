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
            Event e = Event.current;

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);

            GUIStyle outputStyle = new GUIStyle(GUI.skin.label);
            outputStyle.wordWrap = true;

            GUILayout.Label(output, outputStyle);

            GUILayout.EndScrollView();

            //Draw debug rects
            //GUIUtility.DrawRectangle(headerRect, Color.blue);
            GUIUtility.DrawRectangle(resizeRect, Color.red, false);

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

        public override GUIStyle getGUIStyle(GUISkin skin) {
            GUIStyle guiStyle = new GUIStyle(skin.window);

            //Fixed size
            //guiStyle.fixedWidth = dss_DeveloperWindow_rect.size.x;
            //guiStyle.fixedHeight = dss_DeveloperWindow_rect.size.y;

            return guiStyle;
        }

        public override void loadUserSettings() {
            if (isResizable()) {
                setCurrentRect(new Rect(
                    EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_DeveloperWindow_rect.x),
                    EditorPrefs.GetFloat(txt_editorprefs_recty, dss_DeveloperWindow_rect.y),
                    EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_DeveloperWindow_rect.size.x),
                    EditorPrefs.GetFloat(txt_editorprefs_recth, dss_DeveloperWindow_rect.size.y)
                ));
            }
            else {
                setCurrentRect(new Rect(
                    EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_DeveloperWindow_rect.x),
                    EditorPrefs.GetFloat(txt_editorprefs_recty, dss_DeveloperWindow_rect.y),
                    dss_DeveloperWindow_rect.size.x, //Fixed default size
                    dss_DeveloperWindow_rect.size.y //Fixed default size
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
        public static Rect dss_DeveloperWindow_rect = new Rect(0, 0, 400, 200);
        public static Rect dss_DeveloperWindow_minrect = new Rect(0, 0, 100, 40);

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
