using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using System;

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
        protected List<FizzikSubWindow> reversedSubwindows; //Useful for getting the subwindows in order of their depth, from front to back.
        protected ToolPalette toolPalette;
        protected ColorPalette colorPalette;

        protected FizzikSprite workingSprite;

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

            //Load user settings
            editor.loadUserSettings();

            //Instantiate Subwindow classes and their windows
            int subwindowID = wcid_subwindows;

            editor.toolPalette = new ToolPalette();
            editor.toolPalette.setWindowID(subwindowID++);
            editor.subwindows.Add(editor.toolPalette);

            editor.colorPalette = new ColorPalette();
            editor.colorPalette.setWindowID(subwindowID++);
            editor.subwindows.Add(editor.colorPalette);

            editor.reversedSubwindows = new List<FizzikSubWindow>(editor.subwindows);
            editor.reversedSubwindows.Reverse();

            //Set window constraints
            editor.minSize = new Vector2(editor.calculateMinWidth(), editor.calculateMinHeight());

            //Mouse flags
            editor.wantsMouseEnterLeaveWindow = true;
            editor.wantsMouseMove = true;

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
            if (!hasInit) {
                Init();
            }

            Event e = Event.current;

            /****************************************************************
                ******** ---> Handle Mouse Events
                ****************************************************************/
            //DEBUGGING UTILITY BLOCK
            /*if (e.isMouse && e.type == EventType.MouseMove) {
                Vector2 mpos = e.mousePosition;

                foreach (FizzikSubWindow sw in reversedSubwindows) {
                    if (sw.getCurrentRect().Contains(mpos)) {
                        editor.titleContent = new GUIContent("mouseinside");
                        break;
                    }
                    else {
                        editor.titleContent = new GUIContent("mouseoutside");
                    }
                }

                editor.Repaint();
            }*/

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
            editor.titleContent = new GUIContent(txt_Title, EditorGUIUtility.Load("Assets/Scripts/Editor/FizzikAnimation/Images/windowlogo.png") as Texture);

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
            foreach (FizzikSubWindow sw in subwindows) {
                sw.clampInsideRect(new Rect(0f, dss_Toolbar_height, position.size.x, position.size.y - dss_Toolbar_height));
                sw.setCurrentRect(GUI.Window(sw.getWindowID(), sw.getCurrentRect(), sw.handleGUI, sw.getTitle(), sw.getGUIStyle(GUI.skin)));
            }
            EndWindows();
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
            position = new Rect(
                EditorPrefs.GetFloat(txt_editorprefs_rectx, dss_Editor_position.x),
                EditorPrefs.GetFloat(txt_editorprefs_recty, dss_Editor_position.y),
                EditorPrefs.GetFloat(txt_editorprefs_rectw, dss_Editor_position.size.x),
                EditorPrefs.GetFloat(txt_editorprefs_recth, dss_Editor_position.size.y)
            );
        }

        public void saveUserSettings() {
            EditorPrefs.SetFloat(txt_editorprefs_rectx, position.x);
            EditorPrefs.SetFloat(txt_editorprefs_recty, position.y);
            EditorPrefs.SetFloat(txt_editorprefs_rectw, position.size.x);
            EditorPrefs.SetFloat(txt_editorprefs_recth, position.size.y);
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

        protected void handleGUIToolbar(Event e) {
            GUIStyle toolbarStyle = new GUIStyle(EditorStyles.toolbar);

            GUIStyle menuButtonStyle = new GUIStyle(EditorStyles.toolbarButton);
            menuButtonStyle.margin = new RectOffset(0, 0, 0, 0);
            menuButtonStyle.fixedWidth = 100f;
            menuButtonStyle.stretchWidth = false;

            GUILayout.BeginHorizontal(toolbarStyle, GUILayout.Width(editor.position.size.x), GUILayout.Height(dss_Toolbar_height));

            if (GUILayout.Button(new GUIContent("File", ""), menuButtonStyle)) {

            }
            if (GUILayout.Button(new GUIContent("Edit", ""), menuButtonStyle)) {

            }
            if (GUILayout.Button(new GUIContent("View", ""), menuButtonStyle)) {

            }
            if (GUILayout.Button(new GUIContent("Layer", ""), menuButtonStyle)) {

            }

            GUILayout.EndHorizontal();
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_Editor_position = new Rect(150, 150, 1067, 600);
        const float dss_Toolbar_height = 20f;

        /*-------------------------- 
         * ControlId constants
         ---------------------------*/
        const int cid_OpenExistingSprite = 1000;
        const int wcid_subwindows = 2000;

        /*-------------------------- 
         * Text constants
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
    }
}
