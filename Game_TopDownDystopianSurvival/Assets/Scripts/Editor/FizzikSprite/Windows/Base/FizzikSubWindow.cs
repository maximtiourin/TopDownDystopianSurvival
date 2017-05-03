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
    }
}
