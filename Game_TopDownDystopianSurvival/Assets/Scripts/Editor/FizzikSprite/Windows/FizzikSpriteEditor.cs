using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;
using System.Linq;

namespace Fizzik {
    /*
     * The FizzikSprite Editor window that allows the creation and editing of FizzikSprites
     * @author Maxim Tiourin
     *
     * - Cool Idea, allow the marking of certain layers as "hotswappable", which means they are exposed to code and can have their texture replaced by code at will.
     *      This will allow doing stuff like different skins or equipment, etc.
     */
    public class FizzikSpriteEditor : EditorWindow {
        public static FizzikSpriteEditor editor;

        protected List<FizzikSubWindow> subwindows;
        protected List<FizzikMenuOptionsWindow> menuwindows;
        protected ToolPalette toolPalette;
        protected ColorPalette colorPalette;

        protected FizzikSprite workingSprite;

        //Canvas variables
        protected Matrix4x4 previousGUIMatrix;
        const float minCanvasZoom = 1f;
        const float maxCanvasZoom = 30f;
        const float incCanvasZoom = 1f;
        protected Rect canvasZoomArea;
        protected float canvasZoom;
        protected Vector2 canvasZoomOrigin;
        protected bool gridOverlayEnabled;
        protected int gridOverlayCellWidth;
        protected int gridOverlayCellHeight;
        protected Color gridOverlayColor;

        protected Rect previousEditorRect; //Rect used to keep track of any window resizing changes as a result of no OnResize events in unity
        protected bool changesSaved = false; //Flag used in the determining of the current save state of the workingSprite
        protected bool mouseInsideEditor = false; //Flag used to keep track of the mouse being inside/outside the editor

        protected bool hasInit = false;

        /*
         * The hook into the menu that allows the cold-opening of the editor window, will try to open the last opened asset
         * if there was one stored.
         */
        [MenuItem("Window/Fizzik Sprite Editor")]
        public static void Init() {
            editor = getSingletonWindowReference();

            //Instantiate structures
            editor.subwindows = new List<FizzikSubWindow>();
            editor.menuwindows = new List<FizzikMenuOptionsWindow>();

            //Load user settings
            editor.loadUserSettings();

            //Instantiate Subwindow classes and their windows
            int subwindowID = wcid_subwindows;

            editor.toolPalette = new ToolPalette(editor);
            editor.toolPalette.setWindowID(subwindowID++);
            editor.subwindows.Add(editor.toolPalette);

            editor.colorPalette = new ColorPalette(editor);
            editor.colorPalette.setWindowID(subwindowID++);
            editor.subwindows.Add(editor.colorPalette);

            //Set window constraints
            editor.minSize = new Vector2(editor.calculateMinWidth(), editor.calculateMinHeight());
            editor.previousEditorRect = editor.position;

            //Mouse flags
            editor.wantsMouseEnterLeaveWindow = true;
            editor.wantsMouseMove = true;

            //TODO Canvas variables (These might need to get moved around, depending on how I want to handle user settings)
            editor.resetCanvasZoomArea();

            //Load Resources
            editor.tex_windowlogo = Resources.Load<Texture>(editor.rsc_windowlogo);

            string wpath = EditorPrefs.GetString(txt_WorkingSprite_editorprefs_pathkey, txt_WorkingSprite_editorprefs_pathnull);

            if (wpath != "") {
                //Load the workingSprite from the stored path
                //TODO OPEN FIZZIKSPRITE
            }

            editor.hasInit = true;

            editor.Repaint();
        }

        /*
         * Handles the primary logic of displaying and interacting with the Editor window
         */
        void OnGUI() {
            //Enforce init
            if (!hasInit) {
                Init();
            }

            //Track editor resizing event
            if (!position.Equals(previousEditorRect)) {
                OnResize(new ResizeEvent(previousEditorRect, position));
            }

            //Current event QoL helper
            Event e = Event.current;

            //TODO MOUSE ENTER/LEAVE
            if (e.type == EventType.MouseEnterWindow) {
                mouseInsideEditor = true;
            }
            else if (e.type == EventType.MouseLeaveWindow) {
                mouseInsideEditor = false;
            }

            /****************************************************************
             ******** ---> Begin Application content
             ****************************************************************/
            //Window titlebar
            editor.titleContent = new GUIContent(txt_Title, tex_windowlogo);

            //Debug test canvas
            handleCanvas(e);

            //Toolbar
            handleGUIToolbar(e);

            //TODO unreachable code for quick commenting/uncommenting
            if (workingSprite == null) {
                if (false) {
                    /****************************************************************
                     ******** - Check For WorkingSprite from previous open, or display new/open options
                     ****************************************************************/
                    string wpath = EditorPrefs.GetString(txt_WorkingSprite_editorprefs_pathkey, txt_WorkingSprite_editorprefs_pathnull);

                    if (wpath != txt_WorkingSprite_editorprefs_pathnull) {
                        //We still have a workingSprite available from the stored path, reload
                        //TODO OPEN FIZZIKSPRITE
                    }

                    /**-- <LAYOUT> -- **/
                    EditorGUILayout.BeginHorizontal();

                    /****************************************************************
                     ******** - Create New Sprite
                     ****************************************************************/
                    //Create Sprite Button
                    if (GUILayout.Button(new GUIContent(txt_CreateNewSprite_btn_title, txt_CreateNewSprite_btn_ttip))) {
                        string path = EditorUtility.SaveFilePanel(txt_CreateNewSprite_dialog_title,
                            EditorPrefs.GetString(txt_LastUsedSaveDir_editorprefs_pathkey, txt_LastUsedSaveDir_editorprefs_pathdefault),
                            txt_CreateNewSprite_dialog_default, txt_CreateNewSprite_dialog_filetype);

                        if (path != "") {
                            string relpath = FileUtil.GetProjectRelativePath(path);

                            //Path was entered, store the used path and create the asset
                            EditorPrefs.SetString(txt_LastUsedSaveDir_editorprefs_pathkey, Path.GetDirectoryName(path));

                            FizzikSprite spr = ScriptableObject.CreateInstance<FizzikSprite>();

                            AssetDatabase.CreateAsset(spr, relpath);

                            performAssetSave();
                        }
                    }

                    /****************************************************************
                     ******** Open Existing Sprite
                     ****************************************************************/
                    //Open Existing Sprite -- First check to see if one was selected, otherwise act on the Button
                    if (e.type == EventType.ExecuteCommand
                        && e.commandName == "ObjectSelectorUpdated"
                        && EditorGUIUtility.GetObjectPickerControlID() == cid_OpenExistingSprite) {
                        string path = AssetDatabase.GetAssetPath(EditorGUIUtility.GetObjectPickerObject());
                        //Open workingSprite from asset path
                        //TODO OPEN FIZZIKSPRITE

                        //showDebugMessage("Event Fired");

                        //return;
                    }

                    if (GUILayout.Button(new GUIContent(txt_OpenExistingSprite_btn_title, txt_OpenExistingSprite_btn_ttip))) {
                        EditorGUIUtility.ShowObjectPicker<FizzikSprite>(null, false, "", cid_OpenExistingSprite);
                    }

                    /**-- </LAYOUT> -- **/
                    EditorGUILayout.EndHorizontal();
                }
            }
            else {
                /****************************************************************
                 ******** Work on already opened WorkingSprite
                 ****************************************************************/
                //Edit FizzikSprite

                //TODO Subwindows should be displayed here eventually, but for now they are displayed outside of all conditional branches
            }

            //Debug test subwindows
            BeginWindows();
            //Subwindows
            foreach (FizzikSubWindow sw in subwindows) {
                sw.clampInsideRect(new Rect(0f, dss_Toolbar_height, position.size.x, position.size.y - dss_Toolbar_height));
                sw.setCurrentRect(GUI.Window(sw.getWindowID(), sw.getCurrentRect(), sw.handleGUI, sw.getTitle(), sw.getGUIStyle(GUI.skin)));
            }
            EndWindows();

            //Set previous rect
            previousEditorRect = position;
        }

        /*
         * Any cleanup operations that should happen before the window is closed
         */
        void OnDestroy() {
            if (hasInit) {
                //Save editor settings
                saveUserSettings();

                //Save subwindow settings
                foreach (FizzikSubWindow sw in subwindows) {
                    sw.saveUserSettings();
                }

                //Cleanup any remaining menuwindows
                if (menuwindows != null && menuwindows.Count > 0) {
                    foreach (FizzikMenuOptionsWindow mw in menuwindows) {
                        mw.closeWindow();
                    }
                    menuwindows.Clear();
                }
            }
        }

        void OnFocus() {
            //Cleanup any currently open menuwindows
            if (menuwindows != null && menuwindows.Count > 0) {
                foreach (FizzikMenuOptionsWindow mw in menuwindows) {
                    mw.closeWindow();
                }
                menuwindows.Clear();
            }
        }

        /*
         * Custom onResize event that is called in window's OnGUI if different rect dimensions have been detected between OnGUI calls
         */
        void OnResize(ResizeEvent e) {
            if (e.type == ResizeEvent.ResizeEventType.MoveAndResize || e.type == ResizeEvent.ResizeEventType.Resize) {
                //Reset canvas zooming if window has been resized
                resetCanvasZoomArea();
            }
        }

        /*
         * This is called from external assets when they want to be opened from the project browser.
         * This should handle things like prompting the user to save an existing workingSprite if they haven't yet.
         * Then it will return true if the window will now open the calling asset, or false if the user declined to close previous workingSprite.
         */
        public static bool openAssetFromProjectBrowser(UnityEngine.Object obj) {
            if (FizzikSpriteEditor.editor == null) {
                //Init and then open asset
                Init();

                editor.showDebugMessage("Asset opened from browser");

                return true;
            }
            else {
                //Check for saving

                editor.showDebugMessage("Save it!");

                return true;
            }
        }

        /*
         * Handles doing any kind of critical save operations, then sets the save flag to true so that
         * closing the editor, or opening a new file aren't impeded.
         */
        protected void performAssetSave() {
            AssetDatabase.SaveAssets();
            changesSaved = true;
        }

        public void loadUserSettings() {
            //Window Position
            position = new Rect(
                EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_Editor_position.x),
                EditorPrefs.GetFloat(txt_editorprefs_recty, dss_Editor_position.y),
                EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_Editor_position.size.x),
                EditorPrefs.GetFloat(txt_editorprefs_recth, dss_Editor_position.size.y)
            );

            //Grid Overlay
            gridOverlayEnabled = EditorPrefs.GetBool(txt_GridOverlay_editorprefs_pathkey, bool_GridOverlay_editorprefs_default);
            setGridOverlayCellWidth(EditorPrefs.GetInt(txt_GridOverlay_CellWidth_editorprefs_pathkey, int_GridOverlay_CellWidth_editorprefs_default));
            setGridOverlayCellHeight(EditorPrefs.GetInt(txt_GridOverlay_CellHeight_editorprefs_pathkey, int_GridOverlay_CellHeight_editorprefs_default));
            setGridOverlayColor(
                new Color(
                    EditorPrefs.GetFloat(txt_GridOverlay_ColorR_editorprefs_pathkey, float_GridOverlay_ColorR_editorprefs_default),
                    EditorPrefs.GetFloat(txt_GridOverlay_ColorG_editorprefs_pathkey, float_GridOverlay_ColorG_editorprefs_default),
                    EditorPrefs.GetFloat(txt_GridOverlay_ColorB_editorprefs_pathkey, float_GridOverlay_ColorB_editorprefs_default),
                    EditorPrefs.GetFloat(txt_GridOverlay_ColorA_editorprefs_pathkey, float_GridOverlay_ColorA_editorprefs_default)
                )
            );
        }

        public void saveUserSettings() {
            //Window position
            EditorPrefs.SetFloat(txt_editorprefs_rectx, position.x);
            EditorPrefs.SetFloat(txt_editorprefs_recty, position.y);
            EditorPrefs.SetFloat(txt_editorprefs_rectw, position.size.x);
            EditorPrefs.SetFloat(txt_editorprefs_recth, position.size.y);

            //Grid Overlay
            EditorPrefs.SetBool(txt_GridOverlay_editorprefs_pathkey, gridOverlayEnabled);
            EditorPrefs.SetInt(txt_GridOverlay_CellWidth_editorprefs_pathkey, gridOverlayCellWidth);
            EditorPrefs.SetInt(txt_GridOverlay_CellHeight_editorprefs_pathkey, gridOverlayCellHeight);
            EditorPrefs.SetFloat(txt_GridOverlay_ColorR_editorprefs_pathkey, gridOverlayColor.r);
            EditorPrefs.SetFloat(txt_GridOverlay_ColorG_editorprefs_pathkey, gridOverlayColor.g);
            EditorPrefs.SetFloat(txt_GridOverlay_ColorB_editorprefs_pathkey, gridOverlayColor.b);
            EditorPrefs.SetFloat(txt_GridOverlay_ColorA_editorprefs_pathkey, gridOverlayColor.a);
        }

        /*
         * Brings the editor window up, while also returning a reference to it.
         */
        public static FizzikSpriteEditor getSingletonWindowReference() {
            return (FizzikSpriteEditor) EditorWindow.GetWindow(typeof(FizzikSpriteEditor), false, txt_Title);
        }

        /*
         * Displays a debugging dialog box with a 'DEBUG' title, the argument as a message, and a button that says "Dismiss"
         */
        protected void showDebugMessage(string msg) {
            EditorUtility.DisplayDialog("DEBUG", msg, "Dismiss");
        }

        /*
         * Calculates the minWidth allowable for this window by comparing various widths used for styling things such as subwindows, etc.
         */
        protected float calculateMinWidth() {
            float minWidth = 0;

            foreach (FizzikSubWindow sw in subwindows) {
                minWidth = Mathf.Max(minWidth, sw.getCurrentRect().size.x);
            }

            return minWidth;
        }

        /*
         * Calculates the minHeight allowable for this window by comparing various heights used for styling things such as subwindows, etc.
         */
        protected float calculateMinHeight() {
            float minHeight = dss_Toolbar_height;

            foreach (FizzikSubWindow sw in subwindows) {
                minHeight = Mathf.Max(minHeight, dss_Toolbar_height + sw.getCurrentRect().size.y);
            }

            return minHeight;
        }

        /*
         * Zooms a rect based on a scalar, taking into account EditorWindow OnGUI begingroup matrix workaround
         */
        protected Rect beginZoomArea(float zoomScale, Rect area) {
            GUI.EndGroup();

            Rect zoomArea = RectUtility.scaleBy(area, 1f / zoomScale, RectUtility.topLeft(area));
            zoomArea.y += dss_EditorWindow_heightOffset;

            GUI.BeginGroup(zoomArea);

            previousGUIMatrix = GUI.matrix;
            Matrix4x4 translate = Matrix4x4.TRS(RectUtility.topLeft(zoomArea), Quaternion.identity, Vector3.one);
            Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomScale, zoomScale, 1f));
            GUI.matrix = translate * scale * translate.inverse * GUI.matrix;

            return zoomArea;
        }

        protected void endZoomArea() {
            GUI.matrix = previousGUIMatrix;
            GUI.EndGroup();
            GUI.BeginGroup(new Rect(0f, dss_EditorWindow_heightOffset, Screen.width, Screen.height));
        }

        public void resetCanvasZoomArea() {
            canvasZoom = 1f;
            canvasZoomOrigin = Vector2.zero; //Reset the zoom origin
            canvasZoomArea = new Rect(0f, 0f + dss_Toolbar_height, position.size.x, position.size.y - dss_Toolbar_height);
        }

        protected Vector2 getWindowCoordsToZoomCoords(Vector2 windowCoords) {
            return (windowCoords - RectUtility.topLeft(canvasZoomArea)) / canvasZoom + canvasZoomOrigin;
        }

        protected Vector2 floorVec(Vector2 vec) {
            return new Vector2(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y));
        }

        /*
         * TODO, lots of things to implement, currently just testing zoom/panning of prototype canvas
         */
        protected void handleCanvas(Event e) {
            //Handle zooming from mousewheel scrolling event
            if (e.type == EventType.ScrollWheel) {
                Vector2 mousePos = e.mousePosition;
                Vector2 delta = e.delta;
                Vector2 zoomMousePos = getWindowCoordsToZoomCoords(mousePos);
                float scrollNormalizedDelta = -(delta.y / Mathf.Abs(delta.y));

                float prevCanvasZoom = canvasZoom;
                canvasZoom = Mathf.Clamp(canvasZoom + (scrollNormalizedDelta * incCanvasZoom), minCanvasZoom, maxCanvasZoom);
                canvasZoomOrigin += (zoomMousePos - canvasZoomOrigin) - (prevCanvasZoom / canvasZoom) * (zoomMousePos - canvasZoomOrigin);
                //canvasZoomOrigin = floorVec(canvasZoomOrigin);

                e.Use();
            }
            
            //Handle panning from mousewheel dragging event
            if (e.type == EventType.MouseDrag && e.button == 2) {
                const float zoomCurve = .12f;

                Vector2 delta = -e.delta;
                delta /= canvasZoom;
                canvasZoomOrigin += delta * canvasZoom * Mathf.Lerp(1f, zoomCurve, (canvasZoom - minCanvasZoom) / (maxCanvasZoom - minCanvasZoom));
                //canvasZoomOrigin = floorVec(canvasZoomOrigin);

                e.Use();
            }

            //Debug TODO track mouse pos
            Vector2 canvasMousePos = getWindowCoordsToZoomCoords(e.mousePosition);
            float mousex = canvasMousePos.x;
            float mousey = canvasMousePos.y;

            //Canvas content
            beginZoomArea(canvasZoom, canvasZoomArea);

            int hw = Mathf.FloorToInt(canvasZoomArea.size.x / 2f);
            int hh = Mathf.FloorToInt(canvasZoomArea.size.y / 2f);
            int boxw = 300;
            int boxh = 100;
            int hboxw = Mathf.FloorToInt(boxw / 2f);
            int hboxh = Mathf.FloorToInt(boxh / 2f);
            Vector2 zo = canvasZoomOrigin; //Zoom origin QoL helper

            string n = "\n";
            string c = ", ";

            Rect boxRect = new Rect(hw - hboxw - zo.x, hh - hboxh - zo.y, boxw, boxh);
            int boxmousex = (int) (mousex - boxRect.x - zo.x); //Makes sure to offset the zoomOrigin so that the box is always zerod at top left
            int boxmousey = (int) (mousey - boxRect.y - zo.y);

            GUI.Box(boxRect, 
                "Event Mouse Pos: {" + mousex + c + mousey + "}" + n +
                "Canvas Rect: {" + canvasZoomArea.x + c + canvasZoomArea.y + c + canvasZoomArea.size.x + c + canvasZoomArea.size.y + "}" + n +
                "Zoom Origin: {" + canvasZoomOrigin.x + c + canvasZoomOrigin.y + "}" + n +
                "Debug Box Mouse Pos: {" + boxmousex + c + boxmousey + "}"
                );

            //Grid Overlay TODO DEBUG (Possibly Temp)
            if (gridOverlayEnabled) {
                const int gridOffset = 2;
                Texture2D gridOverlay = new Texture2D(boxw + gridOffset, boxh + gridOffset);
                gridOverlay.SetPixels(Enumerable.Repeat(Color.clear, (boxw + gridOffset) * (boxh + gridOffset)).ToArray());
                gridOverlay.filterMode = FilterMode.Point;
                Rect gridRect = new Rect(boxRect.x - (gridOffset / 2), boxRect.y - (gridOffset / 2), boxw + gridOffset, boxh + gridOffset);
                int gridxspc = gridOverlayCellWidth; //Grid line every n pixels
                int gridyspc = gridOverlayCellHeight;
                Color gridColor = gridOverlayColor;
                for (int row = 0; row < boxh + gridOffset; row++) {
                    for (int col = 0; col < boxw + gridOffset; col++) {
                        if (row % gridyspc == 0 || col % gridxspc == 0 || (row == boxh + gridOffset - 1) || (col == boxw + gridOffset - 1)) {
                            Texture2DUtility.SetPixel(gridOverlay, col, row, gridColor);
                        }
                    }
                }
                gridOverlay.Apply();

                GUI.DrawTexture(gridRect, gridOverlay);
            }

            //Pixel Hover Overlay TODO DEBUG (Possibly Temp)
            Texture2D hoverOverlay = new Texture2D(boxw, boxh);
            hoverOverlay.SetPixels(Enumerable.Repeat(Color.clear, boxw * boxh).ToArray());
            hoverOverlay.filterMode = FilterMode.Point;
            if (boxmousex >= 0 && boxmousex < boxw && boxmousey >= 0 && boxmousey < boxh) {
                //Mouse is inside of image, display pixel overlay
                int px = Mathf.FloorToInt(boxmousex);
                int py = Mathf.FloorToInt(boxmousey); //Texture coords origin is bottom left, window coords origin is top left, so flip

                Color ocolor = new Color(1f, 0f, 0f, .5f);

                //TODO DEBUG hacky draw pixel outline
                for (int row = py - 1; row < py - 1 + 3; row++) {
                    for (int col = px - 1; col < px - 1 + 3; col++) {
                        if (row >= 0 && row < boxh && col >= 0 && col < boxw && !(row == py && col == px) && !((int) Mathf.Abs(row - py) == (int) Mathf.Abs(col - px))) {
                            Texture2DUtility.SetPixel(hoverOverlay, col, row, ocolor);
                        }
                    }
                }
            }
            hoverOverlay.Apply();

            GUI.DrawTexture(boxRect, hoverOverlay);

            endZoomArea();

            editor.Repaint(); //Debug Repaint
        }

        protected void handleGUIToolbar(Event e) {
            const float toolbarMenuOffset = -4f;

            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbar);
            toolbarStyle.padding = new RectOffset(0, 0, 0, 0);

            GUIStyle menuButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            menuButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            menuButtonStyle.fixedWidth = 100f;
            menuButtonStyle.stretchWidth = false;

            GUILayout.BeginHorizontal(toolbarStyle, GUILayout.Width(editor.position.size.x), GUILayout.Height(dss_Toolbar_height));

            float btnWidthOffset = 0f;
            if (GUILayout.Button(new GUIContent("File", ""), menuButtonStyle)) {
                Rect btnRect = GUILayoutUtility.GetLastRect();
                btnRect = new Rect(btnRect.x + btnWidthOffset, btnRect.y + dss_Toolbar_height + toolbarMenuOffset, btnRect.size.x, btnRect.size.y);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Create New Sprite"), false, () => {
                    
                });
                menu.AddItem(new GUIContent("Open Existing Sprite"), false, () => {

                });

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Export Frames to PNGs"), false, () => {

                });

                menu.DropDown(btnRect);
            }
            btnWidthOffset += menuButtonStyle.fixedWidth;
            if (GUILayout.Button(new GUIContent("Edit", ""), menuButtonStyle)) {

            }
            btnWidthOffset += menuButtonStyle.fixedWidth;
            if (GUILayout.Button(new GUIContent("View", ""), menuButtonStyle)) {
                Rect btnRect = GUILayoutUtility.GetLastRect();
                btnRect = new Rect(btnRect.x + btnWidthOffset, btnRect.y + dss_Toolbar_height + toolbarMenuOffset, btnRect.size.x, btnRect.size.y);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Grid Options"), false, () => {
                    GridOverlayOptions options = new GridOverlayOptions(this);
                    menuwindows.Add(options);
                    options.showOptions();
                });

                menu.DropDown(btnRect);
            }
            btnWidthOffset += menuButtonStyle.fixedWidth;
            if (GUILayout.Button(new GUIContent("Layer", ""), menuButtonStyle)) {

            }
            btnWidthOffset += menuButtonStyle.fixedWidth;
            if (GUILayout.Button(new GUIContent("Window", ""), menuButtonStyle)) {

            }
            btnWidthOffset += menuButtonStyle.fixedWidth;

            GUILayout.EndHorizontal();
        }

        public bool getGridOverlayEnabled() {
            return gridOverlayEnabled;
        }

        public void setGridOverlayEnabled(bool b) {
            gridOverlayEnabled = b;
        }

        public void toggleGridOverlay() {
            gridOverlayEnabled = !gridOverlayEnabled;
        }

        public int getGridOverlayCellWidth() {
            return gridOverlayCellWidth;
        }

        public void setGridOverlayCellWidth(int width) {
            gridOverlayCellWidth = (int) Mathf.Max(1, width);
        }

        public int getGridOverlayCellHeight() {
            return gridOverlayCellHeight;
        }

        public void setGridOverlayCellHeight(int height) {
            gridOverlayCellHeight = (int) Mathf.Max(1, height);
        }

        public Color getGridOverlayColor() {
            return gridOverlayColor;
        }

        public void setGridOverlayColor(Color color) {
            gridOverlayColor = color;
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_Editor_position = new Rect(150, 150, 1067, 600);
        const float dss_Toolbar_height = 20f;
        const float dss_EditorWindow_heightOffset = 21f;

        /*-------------------------- 
         * ControlId constants
         ---------------------------*/
        const int cid_OpenExistingSprite = 1000;
        const int wcid_subwindows = 2000;

        /*-------------------------- 
         * Editor Textures
         ---------------------------*/
        Texture tex_windowlogo;
        string rsc_windowlogo = "windowlogo";

        /*-------------------------- 
         * Text constants and Editorprefs
         ---------------------------*/
        const string txt_Title = "Sprite Editor";

        const string txt_editorprefs_rectx = "spriteEditor_rectx";
        const string txt_editorprefs_recty = "spriteEditor_recty";
        const string txt_editorprefs_rectw = "spriteEditor_rectw";
        const string txt_editorprefs_recth = "spriteEditor_recth";

        const string txt_LastUsedSaveDir_editorprefs_pathkey = "lastUsedSaveDir";
        const string txt_LastUsedSaveDir_editorprefs_pathdefault = "Assets/";

        const string txt_WorkingSprite_editorprefs_pathkey = "workingSpritePath";
        const string txt_WorkingSprite_editorprefs_pathnull = "";

        const string txt_CreateNewSprite_btn_title = "Create New FizzikSprite";
        const string txt_CreateNewSprite_btn_ttip = "Creates a new FizzikSprite asset. FizzikSprite is an asset that encompasses one or many 2D Textures and can also store special layer and animation based data on a per-frame basis. The frames themselves that can later be exported to common image formats if needed.";
        const string txt_CreateNewSprite_dialog_title = "Select Asset Path";
        const string txt_CreateNewSprite_dialog_default = "MyNewFizzikSprite.asset";
        const string txt_CreateNewSprite_dialog_filetype = "asset";
        const string txt_CreateNewSprite_dialog_msg = "Select where to create the new FizzikSprite asset";

        const string txt_OpenExistingSprite_btn_title = "Open Existing FizzikSprite";
        const string txt_OpenExistingSprite_btn_ttip = "Opens an existing FizzikSprite asset.";
        const string txt_OpenExistingSprite_dialog_title = "Select FizzikSprite";
        const string txt_OpenExistingSprite_dialog_default = "MyNewFizzikSprite.asset";

        const string txt_GridOverlay_editorprefs_pathkey = "gridOverlay";
        const bool bool_GridOverlay_editorprefs_default = false;
        const string txt_GridOverlay_CellWidth_editorprefs_pathkey = "gridOverlayCellWidth";
        const int int_GridOverlay_CellWidth_editorprefs_default = 16;
        const string txt_GridOverlay_CellHeight_editorprefs_pathkey = "gridOverlayCellHeight";
        const int int_GridOverlay_CellHeight_editorprefs_default = 16;
        const string txt_GridOverlay_ColorR_editorprefs_pathkey = "gridOverlayColorR";
        const float float_GridOverlay_ColorR_editorprefs_default = 0f;
        const string txt_GridOverlay_ColorG_editorprefs_pathkey = "gridOverlayColorG";
        const float float_GridOverlay_ColorG_editorprefs_default = 0f;
        const string txt_GridOverlay_ColorB_editorprefs_pathkey = "gridOverlayColorB";
        const float float_GridOverlay_ColorB_editorprefs_default = 0f;
        const string txt_GridOverlay_ColorA_editorprefs_pathkey = "gridOverlayColorA";
        const float float_GridOverlay_ColorA_editorprefs_default = .2f;
    }
}
