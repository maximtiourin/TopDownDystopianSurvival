using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    public interface FizzikSubWindow {
        void handleGUI(int windowID); //This is where the logic and styling for the window is established
        void clampInsideRect(Rect other);
        Rect getCurrentRect();
        void setCurrentRect(Rect rect);
        int getWindowID();
        void setWindowID(int windowID);
        string getTitle();
        GUIStyle getGUIStyle(GUISkin skin);
        void saveUserSettings(); //This is called by the outside parent editorwindow that creates the subwindows
        void loadUserSettings(); //This should be called inside of the subwindow's constructor if it wants to use any saved settings
        void toggleEnabled(); //Negate the value that would be returned by isEnabled()
        bool isEnabled(); //Should return whether or not this subwindow is enabled (the subwindow itself shouldnt do anything with this value, editor will handle choosing what to do)
        Vector2 getRelativeWindowPosition(); //Should return the stored relative window position, this value should have been set by the editor, and will be used by editor
        void setRelativeWindowPosition(Vector2 relpos); //Should be set only by the editor, as it will be used by the editor to reposition the window
        void destroy(); //Should cleanup any memory such as textures
    }
}
