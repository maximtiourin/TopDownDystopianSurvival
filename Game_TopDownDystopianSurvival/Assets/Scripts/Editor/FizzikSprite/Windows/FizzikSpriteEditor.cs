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
     */
    public class FizzikSpriteEditor : EditorWindow {
        public static FizzikSpriteEditor editor;

        protected List<FizzikSubWindow> subwindows;
        protected List<FizzikMenuOptionsWindow> menuwindows;
        protected ToolPalette toolPalette;
        protected ColorPalette colorPalette;
        protected DeveloperWindow devWindow;

        protected FizzikSprite workingSprite; //The currently open fizziksprite

        //Pixel buffering
        protected List<DrawPixelIntent> pixelIntents; //Buffered pixel drawing 
        const int pixelBufferLow = 65536; //256 x 256 :: all areas less than this amount will be subjected to LowRate
        const int pixelBufferHigh  = 262144; //512 x 512 :: all areas greater than equal to Low and less than High will be subjected to linear rate between LowRate and HighRate, greater than equal to High will be subjected to HighRate
        const float pixelBufferLowRate = .05f; //aim to draw buffered intents 20 times a second
        const float pixelBufferHighRate = .2f; //aim to draw buffered intents 5 times a second
        protected float pixelBufferDrawRate = pixelBufferLowRate; //In seconds, can be dynamically scaled to be larger the larger the image is, to prevent losing most stroke data
        protected double nextBufferDraw = 0; //When the next buffer of pixel intents should be drawn in time since unity editor was started

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
        protected int dragContext = -1; //The id of the mouse button that is consequently used as a unique id for drag events, so that once a drag starts, it completes with same mouse button
        protected bool canvasDragAlreadyStarted = false; //Allows drags to continue when hovering over subwindows, if those drags started outside of the subwindow
        protected bool lastToolDragContinuous = false; //Is true as long as a tool drag has been continuously held down, allowing interpolation
        protected Vector2 lastToolDragCoordinate = new Vector2(-9999f, -9999f); //The last coordinate of the mouse that is tracked during a drag in relation to tool events

        //Main editor variables
        const bool DEVELOPER = true; //Used to flag debugging via the developer window
        protected Rect previousEditorRect; //Rect used to keep track of any window resizing changes as a result of not having built-in OnResize events in unity
        protected bool mouseInsideEditor = false; //Flag used to keep track of the mouse being inside/outside the editor
        protected bool shouldDisplayObjectPicker = false; //Flag used to spawn an object picker object inside of the main OnGUI() method, doing so outside of it won't fire events
        protected int displayedObjectPickerId = 0; //The id to assign to the next object picker that should be displayed;
        protected FizzikSubWindow subwindowUnderMouse;

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

            //Pixel buffering
            editor.pixelIntents = new List<DrawPixelIntent>();

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

            if (DEVELOPER) {
                editor.devWindow = new DeveloperWindow(editor);
                editor.devWindow.setWindowID(subwindowID++);
                editor.subwindows.Add(editor.devWindow);
            }

            //Set window constraints
            editor.recalculateMinSize();
            editor.previousEditorRect = editor.position;

            //Mouse flags
            editor.wantsMouseEnterLeaveWindow = true;
            editor.wantsMouseMove = true;

            //TODO Canvas variables (These might need to get moved around, depending on how I want to handle user settings)
            editor.resetCanvasZoomArea();

            //Load Resources
            editor.tex_windowlogo = Resources.Load<Texture>(rsc_windowlogo);
            editor.tex_editorimagebg = Resources.Load<Texture>(rsc_editorimagebg);


            editor.hasInit = true;

            editor.Repaint();
        }

        void Update() {
            //Process Draw Pixel intents
            if (EditorApplication.timeSinceStartup > nextBufferDraw) {
                if (pixelIntents.Count > 0) {
                    HashSet<FizzikLayer> drawnLayers = new HashSet<FizzikLayer>();
                    HashSet<FizzikFrame> drawnFrames = new HashSet<FizzikFrame>();
                    
                    Undo.undoRedoPerformed -= undoRedoWorkingSprite;
                    Undo.undoRedoPerformed += undoRedoWorkingSprite;

                    Undo.RecordObject(workingSprite, "DrawPixel");
                    foreach (DrawPixelIntent intent in pixelIntents) {
                        FizzikSprite sprite = workingSprite;
                        FizzikFrame frame = intent.frame;
                        FizzikLayer layer = intent.layer;

                        if (sprite != null && frame != null && layer != null) {
                            int px = intent.x;
                            int py = intent.y;
                            Color color = intent.color;

                            switch (intent.type) {
                                case (DrawPixelIntent.IntentType.Normal):
                                    Undo.RecordObject(workingSprite, "DrawPixel.Normal");
                                    layer.setPixelTopLeftOrigin(px, py, color, true);
                                    break;
                                case (DrawPixelIntent.IntentType.Interpolate):
                                    Undo.RecordObject(workingSprite, "DrawPixel.Interpolate");
                                    layer.setPixelsInterpolateTopLeftOrigin(intent.prevx, intent.prevy, px, py, color, true);
                                    break;
                                default:
                                    Undo.RecordObject(workingSprite, "DrawPixel.Normal");
                                    layer.setPixelTopLeftOrigin(px, py, color, true);
                                    break;
                            } 
                            
                            drawnLayers.Add(layer);
                        
                            drawnFrames.Add(frame);

                            sprite.offerRecentColor(color);
                        }
                    }

                    foreach (FizzikLayer drawnLayer in drawnLayers) {
                        drawnLayer.updateTexture();
                    }

                    foreach (FizzikFrame drawnFrame in drawnFrames) {
                        drawnFrame.updateTexture();
                    }

                    pixelIntents.Clear();

                    Undo.CollapseUndoOperations(Undo.GetCurrentGroup()); //Group all Records, the group name will be the most recent record

                    makeDirty();
                }

                nextBufferDraw = EditorApplication.timeSinceStartup + pixelBufferDrawRate;
            }
        }

        /*
         * Handles the primary logic of displaying and interacting with the Editor window
         */
        void OnGUI() {
            //Enforce init
            if (!hasInit) {
                Init();
            }

            //Figure out minSizes :: before window resizing events are thrown
            recalculateMinSize();

            //Developer Window DEBUG
            if (DEVELOPER && devWindow.isEnabled()) {
                devWindow.clear();
            }

            //Current event QoL helper
            Event e = Event.current;

            //Save Subwindow position relative to editor boundaries (to be later used in relative reposition on resize)
            //Also use this loop to keep track of the currently hovered over subwindow (so that events down the line can react accordingly)
            //The way that sw's are looped through and drawn, due to 'bringwindowtofront' inside of the loop, the subwindowUnderMouse will always be the window at the forefront.
            subwindowUnderMouse = null;
            foreach (FizzikSubWindow sw in subwindows) {
                //Layer windows in order of their creation always, to aid in tracking the correct forefront window under mouse
                GUI.BringWindowToFront(sw.getWindowID());

                //Relative positioning
                Vector2 curpos = new Vector2(sw.getCurrentRect().x, sw.getCurrentRect().y);
                Vector2 prevsize = new Vector2(previousEditorRect.size.x, previousEditorRect.size.y);
                Vector2 relpos = new Vector2((prevsize.x - curpos.x), prevsize.y - curpos.y);
                sw.setRelativeWindowPosition(relpos);

                //Track window under mouse
                if (sw.isEnabled() && sw.getCurrentRect().Contains(e.mousePosition)) {
                    subwindowUnderMouse = sw;
                }
            }

            //Developer Window DEBUG
            if (DEVELOPER && devWindow.isEnabled()) {
                if (subwindowUnderMouse != null) {
                    devWindow.appendLine("Subwindow under mouse: " + subwindowUnderMouse.getTitle() + " (" + subwindowUnderMouse.getWindowID() + ")");
                }
                else {
                    devWindow.appendLine("Subwindow under mouse: <none>");
                }

                devWindow.appendLine("Pixel Buffer Draw Rate: " + pixelBufferDrawRate);
            }

            //Track editor resizing event
            if (!position.Equals(previousEditorRect)) {
                OnResize(new ResizeEvent(previousEditorRect, position));
            }

            //TODO MOUSE ENTER/LEAVE
            if (e.type == EventType.MouseEnterWindow) {
                mouseInsideEditor = true;
            }
            else if (e.type == EventType.MouseLeaveWindow) {
                mouseInsideEditor = false;
            }

            //Handle object picker selection (has to be done this way because object pickers created outside of OnGUI (even by nested methods) do not send events properly)
            if (shouldDisplayObjectPicker) {
                if (displayedObjectPickerId == cid_OpenExistingSprite) {
                    EditorGUIUtility.ShowObjectPicker<FizzikSprite>(null, false, "", displayedObjectPickerId);
                }

                shouldDisplayObjectPicker = false;
            }
            if (e.type == EventType.ExecuteCommand && e.commandName == "ObjectSelectorClosed") {
                if (EditorGUIUtility.GetObjectPickerControlID() == cid_OpenExistingSprite) {
                    //Open Existing Sprite Object Selected
                    UnityEngine.Object pickerObject = EditorGUIUtility.GetObjectPickerObject();

                    if (pickerObject != null) {
                        string path = AssetDatabase.GetAssetPath(pickerObject);

                        openExistingSprite(path);
                    }
                }
            }

            /****************************************************************
             ******** ---> Begin Application content
             ****************************************************************/
            //Window titlebar
            editor.titleContent = new GUIContent(txt_Title, tex_windowlogo);

            //Canvas
            if (haveWorkingSprite()) {
                handleCanvas(e);
            }

            //Toolbar
            handleGUIToolbar(e);

            //Subwindows
            if (haveWorkingSprite()) {
                BeginWindows();
                foreach (FizzikSubWindow sw in subwindows) {
                    if (sw.isEnabled()) {
                        sw.resizeWindow();
                        sw.clampInsideRect(new Rect(0f, dss_Toolbar_height, position.size.x, position.size.y - dss_Toolbar_height));
                        sw.setCurrentRect(GUI.Window(sw.getWindowID(), sw.getCurrentRect(), sw.handleGUI, sw.getTitle(), sw.getGUIStyle(GUI.skin)));
                    }
                }
                EndWindows();
            }

            //Set previous rect
            previousEditorRect = position;
        }

        /*
         * Any cleanup operations that should happen before the window is closed
         */
        void OnDestroy() {
            if (hasInit) {
                //Clear intents
                pixelIntents.Clear();

                //Save working sprite
                if (workingSprite) {
                    makeDirty();
                    performAssetSave();
                    workingSprite.destroyTextures();
                }

                //Save editor settings
                saveUserSettings();

                //Save subwindow settings
                foreach (FizzikSubWindow sw in subwindows) {
                    sw.saveUserSettings();
                    sw.destroy();
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

                //Reset relative window positions for QoL anchoring
                foreach (FizzikSubWindow sw in subwindows) {
                    Vector2 relpos = sw.getRelativeWindowPosition();

                    sw.setCurrentRect(new Rect(position.size.x - relpos.x, position.size.y - relpos.y, sw.getCurrentRect().size.x, sw.getCurrentRect().size.y));
                }
            }
        }

        protected void offerDrawPixelIntent(DrawPixelIntent.IntentType type, FizzikFrame frame, FizzikLayer layer, int px, int py, Color color) {
            pixelIntents.Add(new DrawPixelIntent(type, frame, layer, px, py, color));
        }

        protected void offerDrawPixelIntent(DrawPixelIntent.IntentType type, FizzikFrame frame, FizzikLayer layer, int prevpx, int prevpy, int px, int py, Color color) {
            pixelIntents.Add(new DrawPixelIntent(type, frame, layer, prevpx, prevpy, px, py, color));
        }

        /*
         * This is called from external assets when they want to be opened from the project browser.
         * It will return true if the window will now open the calling asset, or false if the user declined to close previous workingSprite.
         */
        public static bool openAssetFromProjectBrowser(UnityEngine.Object obj) {
            if (obj != null) {
                if (editor == null) {
                    Init();
                }

                string path = AssetDatabase.GetAssetPath(obj);

                editor.openExistingSprite(path);

                editor.Focus();

                return true;
            }
            else {
                return false;
            }
        }

        /*
         * Handles doing any kind of critical save operations, then sets the save flag to true so that
         * closing the editor, or opening a new file aren't impeded.
         */
        public void performAssetSave() {
            if (haveWorkingSprite()) {
                AssetDatabase.SaveAssets();
            }
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

        protected Vector2 getZoomCoordsToWindowCoords(Vector2 zoomCoords) {
            return ((zoomCoords - canvasZoomOrigin) * canvasZoom) + RectUtility.topLeft(canvasZoomArea);
        }

        protected Vector2 floorVec(Vector2 vec) {
            return new Vector2(Mathf.FloorToInt(vec.x), Mathf.FloorToInt(vec.y));
        }

        /*
         * TODO, lots of things to implement, currently just testing zoom/panning of prototype canvas
         */
        protected void handleCanvas(Event e) {
            //Drag Context
            const int SUBWINDOW = -2;
            const int NONE = -1;
            const int MOUSE_LEFT = 0;
            const int MOUSE_RIGHT = 1;
            const int MOUSE_MIDDLE = 2;

            //Handle preventing cancelling of a subwindow drag if mouse moves back into canvas too quickly. Dont consume input since it needs to be passed to subwindow
            if ((dragContext == NONE) && (subwindowUnderMouse != null || canvasDragAlreadyStarted) && e.type == EventType.MouseDrag && e.button == MOUSE_LEFT) {
                dragContext = SUBWINDOW;

                canvasDragAlreadyStarted = true;

                lastToolDragContinuous = false;
            }

            //Handle zooming from mousewheel scrolling event
            if ((subwindowUnderMouse == null) && e.type == EventType.ScrollWheel) {
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
            if ((dragContext == NONE || dragContext == MOUSE_MIDDLE) && (subwindowUnderMouse == null || canvasDragAlreadyStarted) && e.type == EventType.MouseDrag && e.button == MOUSE_MIDDLE) {
                dragContext = MOUSE_MIDDLE;

                //const float zoomCurve = .12f;

                Vector2 delta = -e.delta;
                canvasZoomOrigin += (delta / canvasZoom);
                //delta /= canvasZoom;
                //canvasZoomOrigin += delta * canvasZoom * Mathf.Lerp(1f, zoomCurve, (canvasZoom - minCanvasZoom) / (maxCanvasZoom - minCanvasZoom));
                //canvasZoomOrigin = floorVec(canvasZoomOrigin);

                canvasDragAlreadyStarted = true;

                e.Use();
            }

            /*
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             * Initialize all helper variables (must be done all at once in this unseemingly chunk to allow for easier debugging
             * -----------------------------------------------------------------------------------------------------------------
             * Goal was to be able to draw single pixel overlays regardless of zoom level.
             * Due to not really understanding how the GUI.matrix scaling was taken into effect, I resorted to
             * Voodoo  magic to get the desired result by playing around with offset numbers until they worked.
             * -----------------------------------------------------------------------------------------------------------------
             */
            
            //Various mouse coordinates
            float mousex = e.mousePosition.x;
            float mousey = e.mousePosition.y;

            Vector2 canvasMousePos = getWindowCoordsToZoomCoords(e.mousePosition);
            float canvasmousex = canvasMousePos.x;
            float canvasmousey = canvasMousePos.y;

            //Canvas size helpers
            Rect cza = canvasZoomArea;
            float ctw = canvasZoomArea.size.x;
            float cth = canvasZoomArea.size.y;
            float hctw = ctw / 2f;
            float hcth = cth / 2f;
            Vector2 zo = canvasZoomOrigin; //Zoom origin QoL helper

            //Working Sprite Texture box
            int boxw = workingSprite.imgWidth;
            int boxh = workingSprite.imgHeight;
            float hboxw = boxw / 2f;
            float hboxh = boxh / 2f;
            Rect boxRect = new Rect(hctw - hboxw - zo.x, hcth - hboxh - zo.y, boxw, boxh); //These values make sense only inside of the zoom GUI.matrix farther below

            //Voodoo magic starts here
            float ho = dss_Toolbar_height; //Toolbar offset
            float gw = boxRect.width;
            float gh = boxRect.height;
            Rect grect = new Rect(boxRect.x - (zo.x * canvasZoom), boxRect.y - (zo.y * canvasZoom), gw, gh);
            Rect sgrect = RectUtility.scaleBy(grect, canvasZoom);
            float sgw = sgrect.width;
            float sgh = sgrect.height;
            float hsgw = sgw / 2f;
            float hsgh = sgh / 2f;
            float voodoo_x_round = canvasZoom;
            float voodoo_y_round = canvasZoom;
            float voodoo_x = (sgrect.x + zo.x + (ctw * ((canvasZoom - 1f) / 2f)));
            voodoo_x = Mathf.Round(voodoo_x / voodoo_x_round) * voodoo_x_round;
            float voodoo_y = (sgrect.y + zo.y + (cth * ((canvasZoom - 1f) / 2f)));
            voodoo_y = Mathf.Round(voodoo_y / voodoo_y_round) * voodoo_y_round;
            sgrect = new Rect(voodoo_x, voodoo_y + ho, sgrect.width, sgrect.height); //Future reference - Key was to add the 'ho' offset AFTER all scaling, instead of before

            //Working Sprite Texture mouse coordinates
            float boxmousexraw = (mousex - sgrect.x) / canvasZoom;
            float boxmouseyraw = (mousey - sgrect.y) / canvasZoom;
            int boxmousex = Mathf.FloorToInt(boxmousexraw);
            int boxmousey = Mathf.FloorToInt(boxmouseyraw);

            /*
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             */

            /*
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             * Handle all other events that rely on helper variables being defined (things such as drawing tools, etc)
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             */
            /*
             * Per-Pixel drawing tools
             */
            // This conditional makes sure that the mouse is inside the canvas area before checking events, so that the window toolbar can still be used
            if (mousex >= cza.x && mousex < cza.x + ctw && mousey >= cza.y && mousey < cza.y + cth) {
                // Test debug click related pixel drawing (using left mouse) :: TODO - make sure valid tools are selected, perform tool relative actions
                if ((dragContext == NONE) && (subwindowUnderMouse == null) && e.type == EventType.MouseUp && e.button == MOUSE_LEFT) {
                    //Check to make sure we aren't redrawing the same pixel within the same drag, make sure the pixel we try to draw is inside the box
                    if (boxmousex >= 0 && boxmousex < boxw && boxmousey >= 0 && boxmousey < boxh) {
                        FizzikFrame frame = workingSprite.getFrame(0);
                        FizzikLayer layer = frame.getLayer(0);
                        offerDrawPixelIntent(DrawPixelIntent.IntentType.Normal, frame, layer, boxmousex, boxmousey, colorPalette.color);

                        e.Use(); //This use has to be internal, because the action becomes valid only inside of these constraints, otherwise left mouse should be freed up
                    }
                }
                // Test debug drag related pixel drawing (using left mouse)
                if ((dragContext == NONE || dragContext == MOUSE_LEFT) && (subwindowUnderMouse == null || canvasDragAlreadyStarted) && e.type == EventType.MouseDrag && e.button == MOUSE_LEFT) {
                    dragContext = MOUSE_LEFT;

                    //Check to make sure we aren't redrawing the same pixel within the same drag, make sure the pixel we try to draw is inside the box
                    if ((((int) lastToolDragCoordinate.x != boxmousex) || ((int) lastToolDragCoordinate.y != boxmousey))
                        && (boxmousex >= 0 && boxmousex < boxw && boxmousey >= 0 && boxmousey < boxh)) {
                        FizzikFrame frame = workingSprite.getFrame(0);
                        FizzikLayer layer = frame.getLayer(0);
                        
                        if (!lastToolDragContinuous) {
                            lastToolDragCoordinate = new Vector2(boxmousex, boxmousey);

                            lastToolDragContinuous = true;
                        }

                        Vector2 last = lastToolDragCoordinate;

                        offerDrawPixelIntent(DrawPixelIntent.IntentType.Interpolate, frame, layer, (int) last.x, (int) last.y, boxmousex, boxmousey, colorPalette.color);
                    }

                    lastToolDragCoordinate = new Vector2(boxmousex, boxmousey);

                    canvasDragAlreadyStarted = true;

                    e.Use();
                }
            }

            /*
             * -----------------------------------------------------------------------------------------------------------------
             * Reset drag context flags, must be done at the end of all mouse related input so no funny business happens
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             */
            
            //Subwindow dragging
            if ((dragContext == SUBWINDOW) && canvasDragAlreadyStarted && e.type == EventType.MouseUp && e.button == MOUSE_LEFT) {
                canvasDragAlreadyStarted = false;
                dragContext = NONE;
                lastToolDragContinuous = false;
            }
            if ((dragContext == SUBWINDOW) && canvasDragAlreadyStarted && !mouseInsideEditor) {
                canvasDragAlreadyStarted = false;
                dragContext = NONE;
                lastToolDragContinuous = false;
            }

            //Mouse Pan dragging
            if ((dragContext == MOUSE_MIDDLE) && canvasDragAlreadyStarted && e.type == EventType.MouseUp && e.button == MOUSE_MIDDLE) {
                canvasDragAlreadyStarted = false;
                dragContext = NONE;
                lastToolDragContinuous = false;

                e.Use();
            }

            //Mouse draw dragging
            if ((dragContext == MOUSE_LEFT) && canvasDragAlreadyStarted && e.type == EventType.MouseUp && e.button == MOUSE_LEFT) {
                canvasDragAlreadyStarted = false;
                dragContext = NONE;
                lastToolDragContinuous = false;

                e.Use();
            }

            /*
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             * -----------------------------------------------------------------------------------------------------------------
             */

            //Developer Window DEBUG
            if (DEVELOPER && devWindow.isEnabled()) {
                devWindow.append("Canvas Dragging: ");
                devWindow.appendLine(canvasDragAlreadyStarted);

                devWindow.append("Canvas DragContext: ");
                devWindow.appendLine(dragContext);

                devWindow.append("Canvas ToolDrag Continuous: ");
                devWindow.appendLine(lastToolDragContinuous);

                devWindow.append("Event Mouse Pos: {");
                devWindow.appendCSV(mousex, mousey);
                devWindow.appendLine("}");

                devWindow.append("Canvas Event Mouse Pos: {");
                devWindow.appendCSV(canvasmousex, canvasmousey);
                devWindow.appendLine("}");

                devWindow.append("Canvas Image Box Rect: {");
                devWindow.appendCSV(boxRect.x, boxRect.y, boxRect.size.x, boxRect.size.y);
                devWindow.appendLine("}");

                devWindow.append("Canvas Image Mouse Pos (*): {");
                devWindow.appendCSV(boxmousexraw, boxmouseyraw);
                devWindow.appendLine("}");

                devWindow.append("Canvas Image Mouse Coord (*): {");
                devWindow.appendCSV(boxmousex, boxmousey);
                devWindow.appendLine("}");

                devWindow.append("Pixel Overlay Box Rect: {");
                devWindow.appendCSV(sgrect.x, sgrect.y, sgrect.size.x, sgrect.size.y);
                devWindow.appendLine("}");
            }

            //Canvas content
            beginZoomArea(canvasZoom, canvasZoomArea);

            //Draw workingSprite Transparency Helper background image (use tex coords to tile the texture without scaling it more than its default size)
            float texcoordw = workingSprite.imgWidth / Mathf.Max(4f, workingSprite.imgWidth % tex_editorimagebg.width); //Makes nice tiling both for factor of 2, as well as non-factor
            float texcoordh = workingSprite.imgHeight / Mathf.Max(4f, workingSprite.imgHeight % tex_editorimagebg.height);
            GUI.DrawTextureWithTexCoords(boxRect, tex_editorimagebg, new Rect(0, 0, texcoordw, texcoordh));

            //Draw workingSprite image
            GUI.DrawTexture(boxRect, workingSprite.getTextureFromFrame(0));

            endZoomArea();

            /*
             * BEGIN SINGLE-PIXEL OVERLAYS
             */

            //Draw Grid Overlay
            if (gridOverlayEnabled) {
                Texture2D gridCellTexture = new Texture2D(1, 1);
                gridCellTexture.SetPixel(0, 0, gridOverlayColor);
                gridCellTexture.filterMode = FilterMode.Point;
                gridCellTexture.Apply();

                float cellw = gridOverlayCellWidth * canvasZoom;
                float cellh = gridOverlayCellHeight * canvasZoom;

                //Draw rows
                for (float row = sgrect.yMin; row < sgrect.yMax; row += cellh) {
                    GUI.DrawTextureWithTexCoords(new Rect(sgrect.x, row, sgw, 1), gridCellTexture, new Rect(0, 0, 1, 1));
                }
                GUI.DrawTextureWithTexCoords(new Rect(sgrect.x, sgrect.yMax, sgw, 1), gridCellTexture, new Rect(0, 0, 1, 1)); //Draw closing line (in case height isn't factor of 2)

                //Draw columns
                for (float col = sgrect.xMin; col < sgrect.xMax; col += cellw) {
                    GUI.DrawTextureWithTexCoords(new Rect(col, sgrect.y, 1, sgh), gridCellTexture, new Rect(0, 0, 1, 1));
                }
                GUI.DrawTextureWithTexCoords(new Rect(sgrect.xMax, sgrect.y, 1, sgh), gridCellTexture, new Rect(0, 0, 1, 1));

                DestroyImmediate(gridCellTexture);
            }

            //Draw Pixel Hover Overlay ; TODO - Add color selection options, possibly expand to be different per tool type selected
            Texture2D pixelHoverTexture = new Texture2D(1, 1);
            pixelHoverTexture.SetPixel(0, 0, Color.red);
            pixelHoverTexture.filterMode = FilterMode.Point;
            pixelHoverTexture.Apply();

            if (boxmousex >= 0 && boxmousex < boxw && boxmousey >= 0 && boxmousey < boxh) {
                //Mouse is inside of image, display pixel overlay
                float sideLength = canvasZoom;

                float px = boxmousex * sideLength;
                float py = boxmousey * sideLength;

                Rect prect = new Rect(sgrect.x + px, sgrect.y + py, sgrect.width, sgrect.height);

                GUI.DrawTextureWithTexCoords(new Rect(prect.x, prect.y, sideLength, 1), pixelHoverTexture, new Rect(0, 0, 1, 1));
                GUI.DrawTextureWithTexCoords(new Rect(prect.x + sideLength, prect.y, 1, sideLength), pixelHoverTexture, new Rect(0, 0, 1, 1));
                GUI.DrawTextureWithTexCoords(new Rect(prect.x, prect.y + sideLength, sideLength, 1), pixelHoverTexture, new Rect(0, 0, 1, 1));
                GUI.DrawTextureWithTexCoords(new Rect(prect.x, prect.y, 1, sideLength), pixelHoverTexture, new Rect(0, 0, 1, 1));
            }

            DestroyImmediate(pixelHoverTexture);

            /*
             * END SINGLE-PIXEL OVERLAYS
             */

            editor.Repaint();
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
                    //Display sprite creation options window
                    CreateNewSpriteOptions options = ScriptableObject.CreateInstance<CreateNewSpriteOptions>();
                    options.Init(this);
                    menuwindows.Add(options);
                    options.showOptions();

                    //createNewSprite();
                });
                menu.AddItem(new GUIContent("Open Existing Sprite"), false, () => {
                    displayedObjectPickerId = cid_OpenExistingSprite;
                    shouldDisplayObjectPicker = true;
                });

                menu.AddSeparator("");

                menu.AddItem(new GUIContent("Export Frames to PNGs"), false, () => {

                });

                menu.DropDown(btnRect);
            }
            btnWidthOffset += menuButtonStyle.fixedWidth;
            if (GUILayout.Button(new GUIContent("Edit", ""), menuButtonStyle)) {
                Rect btnRect = GUILayoutUtility.GetLastRect();
                btnRect = new Rect(btnRect.x + btnWidthOffset, btnRect.y + dss_Toolbar_height + toolbarMenuOffset, btnRect.size.x, btnRect.size.y);

                string currentUndoGroupName = Undo.GetCurrentGroupName();

                GenericMenu menu = new GenericMenu();
                if (currentUndoGroupName == "") {
                    menu.AddDisabledItem(new GUIContent("Undo %Z"));
                }
                else {
                    menu.AddItem(new GUIContent("Undo " + currentUndoGroupName + " %Z"), false, () => {
                        Undo.PerformUndo();
                    });
                }

                //For some reason Unity doesn't provide a simple way to get the undoStack, or even the redo groupName, so I can't mimic the unity editor undo/redo menu items 100%...
                menu.AddItem(new GUIContent("Redo %Y"), false, () => {
                    Undo.PerformRedo();
                });

                menu.DropDown(btnRect);
            }
            btnWidthOffset += menuButtonStyle.fixedWidth;
            if (GUILayout.Button(new GUIContent("View", ""), menuButtonStyle)) {
                Rect btnRect = GUILayoutUtility.GetLastRect();
                btnRect = new Rect(btnRect.x + btnWidthOffset, btnRect.y + dss_Toolbar_height + toolbarMenuOffset, btnRect.size.x, btnRect.size.y);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Grid Options"), false, () => {
                    GridOverlayOptions options = ScriptableObject.CreateInstance<GridOverlayOptions>();
                    options.Init(this);
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
                Rect btnRect = GUILayoutUtility.GetLastRect();
                btnRect = new Rect(btnRect.x + btnWidthOffset, btnRect.y + dss_Toolbar_height + toolbarMenuOffset, btnRect.size.x, btnRect.size.y);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(new GUIContent("Color Palette"), colorPalette.isEnabled(), () => {
                    colorPalette.toggleEnabled();
                });
                menu.AddItem(new GUIContent("Tool Palette"), toolPalette.isEnabled(), () => {
                    toolPalette.toggleEnabled();
                });
                if (DEVELOPER) {
                    menu.AddItem(new GUIContent("Developer"), devWindow.isEnabled(), () => {
                        devWindow.toggleEnabled();
                    });
                }

                menu.DropDown(btnRect);
            }
            btnWidthOffset += menuButtonStyle.fixedWidth;

            GUILayout.EndHorizontal();
        }

        /*
         * Creates a new FizzikSprite with width, height,
         * while also setting it to the current working sprite.
         */
        public void createNewSprite(int w, int h) {
            string dirpath = EditorPrefs.GetString(txt_LastUsedSaveDir_editorprefs_pathkey, txt_LastUsedSaveDir_editorprefs_pathdefault);

            string path;
            bool wasrelpath = false;
            if (dirpath == txt_LastUsedSaveDir_editorprefs_pathdefault) {
                //Default to assets root directory
                path = EditorUtility.SaveFilePanelInProject(txt_CreateNewSprite_dialog_title,
                    txt_CreateNewSprite_dialog_default,
                    txt_CreateNewSprite_dialog_filetype, "");

                wasrelpath = true;
            }
            else {
                //Use user's last save directory
                path = EditorUtility.SaveFilePanel(txt_CreateNewSprite_dialog_title,
                dirpath,
                txt_CreateNewSprite_dialog_default, txt_CreateNewSprite_dialog_filetype);

                wasrelpath = false;
            }

            if (path != "") {
                string relpath = (wasrelpath) ? (path) : (FileUtil.GetProjectRelativePath(path));

                //Path was entered, store the used path and create the asset
                EditorPrefs.SetString(txt_LastUsedSaveDir_editorprefs_pathkey, Path.GetDirectoryName(path));

                FizzikSprite spr = ScriptableObject.CreateInstance<FizzikSprite>();

                AssetDatabase.CreateAsset(spr, relpath);

                spr.Init(w, h);

                if (workingSprite) {
                    workingSprite.destroyTextures();
                    pixelIntents.Clear();
                }

                workingSprite = spr;

                makeDirty();

                calibratePixelBufferDrawRate();

                performAssetSave();
            }

            Focus();
        }

        /*
         * Opens an existing fizziksprite inside of the project folder
         */
        public void openExistingSprite(string path) {
            if (workingSprite) {
                workingSprite.destroyTextures();
                pixelIntents.Clear();
            }

            workingSprite = AssetDatabase.LoadAssetAtPath<FizzikSprite>(path);

            workingSprite.reconstructTextures();

            makeDirty();

            calibratePixelBufferDrawRate();

            performAssetSave();

            Focus();
        }

        /*
         * Fulls records the working sprite object, and collapses every record into one undo/redo with the 'operation' label
         */
        protected void recordWorkingSprite(string operation) {
            if (workingSprite) {
                Undo.RecordObject(workingSprite, operation);
            }
        }

        protected void undoRedoWorkingSprite() {
            if (workingSprite) {
                workingSprite.reconstructTextures();
            }
        }

        /*
         * Sets the rate at which pixel draw intents are drawn in batches,
         * taking into account the area of the image. 
         */
        protected void calibratePixelBufferDrawRate() {
            if (workingSprite) {
                int area = workingSprite.imgWidth * workingSprite.imgHeight;

                int lo = pixelBufferLow;
                int hi = pixelBufferHigh;

                float lorate = pixelBufferLowRate;
                float hirate = pixelBufferHighRate;

                float rate = 0f;
                if (area < lo) {
                    rate = lorate;
                }
                else if (area < hi) {
                    float ratio = ((float) area - (float) lo) / ((float) hi - (float) lo);
                    rate = lorate + (ratio * (hirate - lorate));
                }
                else {
                    rate = hirate;
                }

                pixelBufferDrawRate = rate;
            }
        }

        /*
         * Recaculates the minimum size the editor window can be scaled to in order to allow for all of the various offsets and subwindows at their current dimensions
         */
        public void recalculateMinSize() {
            float minWidth = 0;
            float minHeight = dss_Toolbar_height;

            foreach (FizzikSubWindow sw in subwindows) {
                if (sw.isEnabled()) {
                    minWidth = Mathf.Max(minWidth, sw.getCurrentRect().size.x);
                    minHeight = Mathf.Max(minHeight, dss_Toolbar_height + sw.getCurrentRect().size.y);
                }
            }

            minSize = new Vector2(minWidth, minHeight);
        }

        /*
         * Flags the workingSprite to be dirty inside of Unity, so that it is written to disk when unity is closed.
         */
        protected void makeDirty() {
            if (workingSprite != null) {
                EditorUtility.SetDirty(workingSprite);
            }
        }

        public bool haveWorkingSprite() {
            return workingSprite != null;
        }

        public FizzikSprite getWorkingSprite() {
            return workingSprite;
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
        const string rsc_windowlogo = "fizzik_windowlogo";
        Texture tex_editorimagebg;
        const string rsc_editorimagebg = "fizzik_editorimagebg";

        /*-------------------------- 
         * Text constants and Editorprefs
         ---------------------------*/
        const string txt_Title = "Sprite Editor";

        const string txt_editorprefs_rectx = "Fizzik.spriteEditor_rectx";
        const string txt_editorprefs_recty = "Fizzik.spriteEditor_recty";
        const string txt_editorprefs_rectw = "Fizzik.spriteEditor_rectw";
        const string txt_editorprefs_recth = "Fizzik.spriteEditor_recth";

        const string txt_LastUsedSaveDir_editorprefs_pathkey = "Fizzik.lastUsedSaveDir";
        const string txt_LastUsedSaveDir_editorprefs_pathdefault = "Assets/";

        const string txt_WorkingSprite_editorprefs_pathkey = "Fizzik.workingSpritePath";
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

        const string txt_GridOverlay_editorprefs_pathkey = "Fizzik.gridOverlay";
        const bool bool_GridOverlay_editorprefs_default = false;
        const string txt_GridOverlay_CellWidth_editorprefs_pathkey = "Fizzik.gridOverlayCellWidth";
        const int int_GridOverlay_CellWidth_editorprefs_default = 16;
        const string txt_GridOverlay_CellHeight_editorprefs_pathkey = "Fizzik.gridOverlayCellHeight";
        const int int_GridOverlay_CellHeight_editorprefs_default = 16;
        const string txt_GridOverlay_ColorR_editorprefs_pathkey = "Fizzik.gridOverlayColorR";
        const float float_GridOverlay_ColorR_editorprefs_default = 0f;
        const string txt_GridOverlay_ColorG_editorprefs_pathkey = "Fizzik.gridOverlayColorG";
        const float float_GridOverlay_ColorG_editorprefs_default = 0f;
        const string txt_GridOverlay_ColorB_editorprefs_pathkey = "Fizzik.gridOverlayColorB";
        const float float_GridOverlay_ColorB_editorprefs_default = 0f;
        const string txt_GridOverlay_ColorA_editorprefs_pathkey = "Fizzik.gridOverlayColorA";
        const float float_GridOverlay_ColorA_editorprefs_default = .2f;
    }
}
