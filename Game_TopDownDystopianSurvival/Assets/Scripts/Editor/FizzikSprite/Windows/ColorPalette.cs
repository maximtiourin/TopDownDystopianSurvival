using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class ColorPalette : FizzikSubWindow {
        public static string defaultTitle = "Color Palette";

        const int MOUSE_DRAG_BUTTON = 0;

        private FizzikSpriteEditor editor;
        private Vector2 relativePos = Vector2.zero;
        private Rect currentRect;
        private int windowID;
        private bool enabled = true;

        private Texture2D btnImage;

        public Color color;
        private bool colorInit = false;
        private Color[] defaultColors;

        public ColorPalette(FizzikSpriteEditor editor) {
            this.editor = editor;

            //Define white btn background image canvas
            btnImage = new Texture2D(3, 3);
            btnImage.filterMode = FilterMode.Point;
            btnImage.SetPixels(Enumerable.Repeat(Color.black, 3 * 3).ToArray());
            btnImage.SetPixel(1, 1, Color.white);
            btnImage.Apply();

            //Define default colors
            int col = 0;
            defaultColors = Enumerable.Repeat(Color.white, DEFAULTCOLORS_COLUMNS * DEFAULTCOLORS_ROWS).ToArray();
            col = 0;
            setDefaultColor(col, 0, new Color(1f, 1f, 1f, 1f));
            setDefaultColor(col, 1, new Color(.8f, .8f, .8f, 1f));
            setDefaultColor(col, 2, new Color(.6f, .6f, .6f, 1f));
            setDefaultColor(col, 3, new Color(.4f, .4f, .4f, 1f));
            setDefaultColor(col, 4, new Color(.2f, .2f, .2f, 1f));
            setDefaultColor(col, 5, new Color(0f, 0f, 0f, 1f));
            col = 1;
            setDefaultColor(col, 0, new Color(1f, 0f, 0f, 1f));
            setDefaultColor(col, 1, new Color(.6f, 0f, 0f, 1f));
            setDefaultColor(col, 2, new Color(.2f, 0f, 0f, 1f));
            setDefaultColor(col, 3, new Color(134 / 255f, 49 / 255f, 62 / 255f, 1f));
            setDefaultColor(col, 4, new Color(183 / 255f, 100 / 255f, 79 / 255f, 1f));
            setDefaultColor(col, 5, new Color(51 / 255f, 32 / 255f, 11 / 255f, 1f));
            col = 2;
            setDefaultColor(col, 0, new Color(0f, 1f, 0f, 1f));
            setDefaultColor(col, 1, new Color(0f, .6f, 0f, 1f));
            setDefaultColor(col, 2, new Color(0f, .2f, 0f, 1f));
            setDefaultColor(col, 3, new Color(71 / 255f, 105 / 255f, 73 / 255f, 1f));
            setDefaultColor(col, 4, new Color(202 / 255f, 140 / 255f, 125 / 255f, 1f));
            setDefaultColor(col, 5, new Color(81 / 255f, 57 / 255f, 32 / 255f, 1f));
            col = 3;
            setDefaultColor(col, 0, new Color(0f, 0f, 1f, 1f));
            setDefaultColor(col, 1, new Color(0f, 0f, .6f, 1f));
            setDefaultColor(col, 2, new Color(0f, 0f, .2f, 1f));
            setDefaultColor(col, 3, new Color(65 / 255f, 87 / 255f, 139 / 255f, 1f));
            setDefaultColor(col, 4, new Color(218 / 255f, 158 / 255f, 92 / 255f, 1f));
            setDefaultColor(col, 5, new Color(104 / 255f, 80 / 255f, 55 / 255f, 1f));
            col = 4;
            setDefaultColor(col, 0, new Color(1f, 1f, 0f, 1f));
            setDefaultColor(col, 1, new Color(.6f, .6f, 0f, 1f));
            setDefaultColor(col, 2, new Color(.2f, .2f, 0f, 1f));
            setDefaultColor(col, 3, new Color(138 / 255f, 116 / 255f, 49 / 255f, 1f));
            setDefaultColor(col, 4, new Color(228 / 255f, 185 / 255f, 137 / 255f, 1f));
            setDefaultColor(col, 5, new Color(99 / 255f, 50 / 255f, 37 / 255f, 1f));
            col = 5;
            setDefaultColor(col, 0, new Color(0f, 1f, 1f, 1f));
            setDefaultColor(col, 1, new Color(0f, .6f, .6f, 1f));
            setDefaultColor(col, 2, new Color(0f, .2f, .2f, 1f));
            setDefaultColor(col, 3, new Color(90 / 255f, 128 / 255f, 128 / 255f, 1f));
            setDefaultColor(col, 4, new Color(210 / 255f, 184 / 255f, 130 / 255f, 1f));
            setDefaultColor(col, 5, new Color(183 / 255f, 143 / 255f, 70 / 255f, 1f));
            col = 6;
            setDefaultColor(col, 0, new Color(1f, 0f, 1f, 1f));
            setDefaultColor(col, 1, new Color(.6f, 0f, .6f, 1f));
            setDefaultColor(col, 2, new Color(.2f, 0f, .2f, 1f));
            setDefaultColor(col, 3, new Color(88 / 255f, 73 / 255f, 133 / 255f, 1f));
            setDefaultColor(col, 4, new Color(239 / 255f, 203 / 255f, 135 / 255f, 1f));
            setDefaultColor(col, 5, new Color(228 / 255f, 176 / 255f, 69 / 255f, 1f));

            loadUserSettings();
        }

        public void handleGUI(int windowID) {
            Color prevColor = GUI.color;

            FizzikSprite sprite = editor.getWorkingSprite();

            //Set initial color
            if (!colorInit && sprite != null) {
                color = sprite.recentColors[0];
                colorInit = true;
            }

            EditorGUILayout.BeginVertical();

            //Define styles
            GUIStyle btnstyle = new GUIStyle(GUI.skin.button);
            btnstyle.fixedWidth = 20;
            btnstyle.fixedHeight = 20;
            btnstyle.margin = new RectOffset(1, 2, 2, 2);
            btnstyle.border = new RectOffset(1, 1, 1, 1);
            btnstyle.normal.background = btnImage;
            btnstyle.hover.background = btnImage;
            btnstyle.active.background = btnImage;
            btnstyle.focused.background = btnImage;

            GUIStyle btnstyle2 = new GUIStyle(btnstyle);
            btnstyle2.fixedWidth = 13;
            btnstyle2.fixedHeight = 13;

            //Draw Default Color Palette
            for (int j = 0; j < DEFAULTCOLORS_ROWS; j++) {
                EditorGUILayout.BeginHorizontal();

                for (int i = 0; i < DEFAULTCOLORS_COLUMNS; i++) {
                    Color defColor = getDefaultColor(i, j);

                    GUI.color = defColor;

                    if (GUILayout.Button("", btnstyle)) {
                        color = defColor;
                    }
                }

                EditorGUILayout.EndHorizontal();
            }

            //Draw recently selected colors
            if (sprite != null) {
                GUILayout.Space(4f);

                GUILayout.BeginHorizontal();

                GUILayout.Space(2f);

                Color[] recentColors = sprite.recentColors;
                for (int i = 0; i < recentColors.Length; i++) {
                    GUI.color = recentColors[i];

                    if (GUILayout.Button("", btnstyle2)) {
                        color = recentColors[i];
                    }
                }

                GUILayout.EndHorizontal();
            }

            //Draw currently selected color
            GUI.color = Color.white;

            GUILayout.Space(8f);

            color = EditorGUILayout.ColorField(color);



            EditorGUILayout.EndVertical();

            if (Event.current.button == MOUSE_DRAG_BUTTON) {
                GUI.DragWindow();
            }

            GUI.color = prevColor;
        }

        private Color getDefaultColor(int x, int y) {
            return defaultColors[y * DEFAULTCOLORS_COLUMNS + x];
        }

        private void setDefaultColor(int x, int y, Color color) {
            defaultColors[y * DEFAULTCOLORS_COLUMNS + x] = color;
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

        public void destroy() {
            if (btnImage) {
                Object.DestroyImmediate(btnImage);
            }
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
                dss_ColorPalette_rect.size.x,
                dss_ColorPalette_rect.size.y
                //EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_ColorPalette_rect.size.x),
                //EditorPrefs.GetFloat(txt_editorprefs_recth, dss_ColorPalette_rect.size.y)
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
        public static Rect dss_ColorPalette_rect = new Rect(0, 0, 165, 204);

        /*-------------------------- 
         * Layout constants
         ---------------------------*/
        const int DEFAULTCOLORS_COLUMNS = 7;
        const int DEFAULTCOLORS_ROWS = 6;

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_rectx = "Fizzik.colorPalette_rectx";
        const string txt_editorprefs_recty = "Fizzik.colorPalette_recty";
        const string txt_editorprefs_rectw = "Fizzik.colorPalette_rectw";
        const string txt_editorprefs_recth = "Fizzik.colorPalette_recth";
        const string txt_editorprefs_enabled = "Fizzik.colorPalette_enabled";
    }
}
