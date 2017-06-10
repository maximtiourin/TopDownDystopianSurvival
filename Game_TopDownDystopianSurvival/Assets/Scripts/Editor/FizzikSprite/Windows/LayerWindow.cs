using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fizzik {
    public class LayerWindow : FizzikSubWindow {
        const string defaultTitle = "Layers";

        const bool RESIZABLE = true;

        const int MOUSE_DRAG_BUTTON = 0;

        private Vector2 scrollPosition;

        private List<Rect> layerRects; //Layer rects that denote the entire area of the layer, to be used for click events after the layers are determined inside of handleGUI

        private Texture2D pixel;
        private Texture2D layersBackgroundTex;
        private Texture2D layerBackgroundTex;
        private Texture2D layerSelectedBackgroundTex;

        public LayerWindow(FizzikSpriteEditor editor) : base(editor) {
            layerRects = new List<Rect>();

            pixel = new Texture2D(1, 1);
            pixel.SetPixel(0, 0, Color.white);
            pixel.filterMode = FilterMode.Point;
            pixel.Apply();

            layersBackgroundTex = new Texture2D(3, 3);
            layersBackgroundTex.SetPixels(Enumerable.Repeat(Color.blue, 3 * 3).ToArray());
            layersBackgroundTex.filterMode = FilterMode.Point;
            layersBackgroundTex.Apply();

            layerBackgroundTex = new Texture2D(3, 3);
            layerBackgroundTex.SetPixels(Enumerable.Repeat(Color.red, 3 * 3).ToArray());
            layerBackgroundTex.filterMode = FilterMode.Point;
            layerBackgroundTex.Apply();

            layerSelectedBackgroundTex = new Texture2D(3, 3);
            layerSelectedBackgroundTex.SetPixels(Enumerable.Repeat(Color.grey, 3 * 3).ToArray());
            layerSelectedBackgroundTex.filterMode = FilterMode.Point;
            layerSelectedBackgroundTex.Apply();
        }

        public override void handleGUI(int windowID) {
            base.handleGUI(windowID);

            //Reset layerRects
            layerRects.Clear();

            //Layer layouting variables
            FizzikSprite sprite = editor.getWorkingSprite();
            FizzikFrame frame = sprite.getCurrentFrame();
            List<FizzikLayer> layers = frame.layers;

            float w = currentRect.width;
            float h = currentRect.height;
            float hw = w / 2;
            float hh = h / 2;

            //Styles
            GUIStyle scrollViewStyle = new GUIStyle(GUI.skin.scrollView);

            GUIStyle layersStyle = new GUIStyle();
            layersStyle.normal.background = layersBackgroundTex;

            GUIStyle layerStyle = new GUIStyle(GUI.skin.button);
            layerStyle.normal.background = layerBackgroundTex;

            GUIStyle layerSelectedStyle = new GUIStyle(layerStyle);
            layerSelectedStyle.normal.background = layerSelectedBackgroundTex;

            GUIStyle layerNameStyle = new GUIStyle();
            layerNameStyle.fixedWidth = LAYER_NAME_WIDTH;
            layerNameStyle.clipping = TextClipping.Clip;

            GUIStyle layerOverallToolbarStyle = new GUIStyle();

            //Begin GUI
            GUILayout.BeginVertical();
                       
            scrollPosition = GUILayout.BeginScrollView(scrollPosition, scrollViewStyle);

            //Begin Layers
            GUILayout.BeginVertical(layersStyle);

            for (int i = layers.Count() - 1; i >= 0; i--) {
                FizzikLayer layer = layers[i];

                //Begin Layer
                if (frame.getCurrentLayer() == layer) {
                    GUILayout.BeginHorizontal(layerSelectedStyle, GUILayout.Height(LAYER_HEIGHT));
                }
                else {
                    GUILayout.BeginHorizontal(layerStyle, GUILayout.Height(LAYER_HEIGHT));
                }

                GUILayout.Box(layer.texture, new GUIStyle(GUI.skin.button), GUILayout.Width(LAYER_HEIGHT), GUILayout.Height(LAYER_HEIGHT));
                GUILayout.Label(layer.name, layerNameStyle);

                GUILayout.EndHorizontal();
                layerRects.Add(GUILayoutUtility.GetLastRect());
                //End Layer
            }

            GUILayout.EndVertical();
            //End Layers

            //Draw layerRects DEBUG TODO
            foreach (Rect rect in layerRects) {
                GUIUtility.DrawRectangle(rect, Color.cyan, true, pixel);
            }

            GUILayout.EndScrollView();

            //Begin Layer Overall Toolbar
            GUILayout.BeginHorizontal(layerOverallToolbarStyle, GUILayout.Height(20));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+", GUILayout.Width(16), GUILayout.Height(16))) {
                frame.createNewLayer();
            }
            if (GUILayout.Button("-", GUILayout.Width(16), GUILayout.Height(16))) {
                frame.deleteCurrentLayer();
            }
            GUILayout.Button("a", GUILayout.Width(16), GUILayout.Height(16));
            GUILayout.Button("b", GUILayout.Width(16), GUILayout.Height(16));

            GUILayout.EndHorizontal();
            //End Layer Overall Toolbar

            GUILayout.EndVertical();

            handleCursors();

            dragWindow();
        }

        public override void destroy() {
            if (pixel) {
                Object.DestroyImmediate(pixel);
            }
            if (layersBackgroundTex) {
                Object.DestroyImmediate(layerBackgroundTex);
            }
            if (layerBackgroundTex) {
                Object.DestroyImmediate(layerBackgroundTex);
            }
            if (layerSelectedBackgroundTex) {
                Object.DestroyImmediate(layerSelectedBackgroundTex);
            }
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
            return dss_LayerWindow_rect;
        }

        public override Rect getMinRect() {
            return dss_LayerWindow_minrect;
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
        public static Rect dss_LayerWindow_rect = new Rect(0, 0, 400, 200);
        public static Rect dss_LayerWindow_minrect = new Rect(0, 0, 180, 80);

        /*-------------------------- 
         * Layout constants
         ---------------------------*/
        const int LAYER_HEIGHT = 40; //The vertical length of a layer
        const int LAYER_WIDTH_OFFSET = -11; //How much the width should be shrunk for the layer after it is scaled to fit the subwindow
        const int LAYER_Y_PADDING = 5; //How much vertical space should exist between layers
        const int LAYER_NAME_WIDTH = 90;

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_editorprefs_identifier = "layerWindow";
    }
}
