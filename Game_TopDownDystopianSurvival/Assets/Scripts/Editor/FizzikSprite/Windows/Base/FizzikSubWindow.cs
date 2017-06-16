using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public abstract class FizzikSubWindow {
        protected const int HEADER_HEIGHT = 15;
        protected const int RESIZE_WIDTH = 4;
        protected const int RESIZE_HEIGHT = 4;

        protected FizzikSpriteEditor editor;
        protected Vector2 relativePos = Vector2.zero;
        protected Rect currentRect; //The bounds of the window
        protected Rect headerRect; //The bounds of the window header
        protected int windowID;
        protected bool enabled = true;

        protected delegate void MouseButtonClickTrackedDelegate(Vector2 clickPosition);
        protected delegate void MouseButtonDraggedDelegate(Vector2 dragPos);
        protected MouseButtonClickTrackedDelegate LeftMouseButtonClickTracked; //Calls entire invocation list when a full click of the left mouse button occurs, e.mousePosition will be inside of rect (0, 0, currentRect.width, currentRect.height)
        protected MouseButtonClickTrackedDelegate RightMouseButtonClickTracked;
        protected MouseButtonClickTrackedDelegate MiddleMouseButtonClickTracked;
        protected MouseButtonDraggedDelegate LeftMouseButtonDragged;
        protected MouseButtonDraggedDelegate RightMouseButtonDragged;
        protected MouseButtonDraggedDelegate MiddleMouseButtonDragged;
        protected MouseButtonDraggedDelegate LeftMouseButtonDragEnded;
        protected MouseButtonDraggedDelegate RightMouseButtonDragEnded;
        protected MouseButtonDraggedDelegate MiddleMouseButtonDragEnded;
        protected const int TRACK_MOUSEBUTTON_COUNT = 3;
        protected bool[] clickTrackMouseDown; //Track if the mouse was recently pressed down
        protected Vector2[] clickTrackMouseDownPos;
        protected float clickTrackPosDeviation = 0f; //d:1.4 How far the mouse can move between mouseDown and mouseUp to register a click
        protected bool clicksTracked = false; //Flag to determine if clicksTracked has been called yet (for use in methods that depend on clicks being tracked)

        protected MouseCursor forcedMouseCursor; //Some actions like resizing require a mouse cursor to stay constant during the action, the cursor type is stored here.

        protected enum ResizeRects {
            TopLeft, Top, TopRight, Left, Right, BotLeft, Bot, BotRight
        }
        protected Rect[] resizeRects; //The bounds of the area where window resizing activates
        protected ResizeRects resizeAnchor; //The resize rect that is being compared to the mouse position
        protected bool resizing;
        protected Vector2 resizeMouseOffset;

        public FizzikSubWindow(FizzikSpriteEditor editor) {
            this.editor = editor;

            //Init click tracking
            clickTrackMouseDown = new bool[] { false, false, false };
            clickTrackMouseDownPos = new Vector2[] { Vector2.zero, Vector2.zero, Vector2.zero };


            setCurrentRect(getDefaultRect()); //Initialization of currentRect that sets up all structures, in case loadUserSettings implementation is blank.

            loadUserSettings();
        }

        /*
         * Handles drawing the gui and updating any important state information, child classes should always call this base method in their implementation at the
         * beginning of that implementation
         */
        public virtual void handleGUI(int windowID) {
            editor.setCurrentMostLikelyFocusedSubwindow(windowID);

            //Reset update state of clicksTracked
            clicksTracked = false;
        }

        protected void dragWindow() {
            Event e = Event.current;

            if (e.button == getMouseDragButton()) {
                GUI.DragWindow(headerRect);
            }
        }

        /*
         * Tracks the state of any full mouse clicks, and calls the MouseButtonClickTracked delegate when a full mouse click is registered
         * This method should be called at start of handleGUI method call if the functionality is desired.
         * This method can be used to track state of mouseDown for all 3 mouse buttons, whether or not it was recently down
         * and at what position it started being down at initially. Also Tracks drag events and drag ended events.
         *
         * Does not consume input on its own, but the delegates it calls may do so.
         */
        protected void trackMouseClicksAndDrags() {
            Event e = Event.current;

            //Track full stationary mouse clicks
            Rect insideRect = new Rect(0, 0, currentRect.width, currentRect.height);
            for (int i = 0; i < TRACK_MOUSEBUTTON_COUNT; i++) {
                if (!clickTrackMouseDown[i] && e.button == i && e.type == EventType.mouseDown) {
                    if (insideRect.Contains(e.mousePosition)) {
                        //Make sure no other clicks are being processed, first come first serve for context clicking, initiate clicks/drags
                        bool valid = true;
                        for (int v = 0; v < TRACK_MOUSEBUTTON_COUNT; v++) {
                            if (v != i) {
                                if (clickTrackMouseDown[v]) {
                                    valid = false; //Another button was clicked first, disqualify this one
                                }
                            }
                        }

                        if (valid) {
                            clickTrackMouseDown[i] = true;
                            clickTrackMouseDownPos[i] = e.mousePosition;
                        }
                    }
                }
                else if (clickTrackMouseDown[i] && e.button == i && e.type == EventType.mouseDrag) {
                    //Drag continued
                    callClickTrackDraggedDelegates(i, e.mousePosition);
                }

                if (clickTrackMouseDown[i] && e.button == i && e.type == EventType.mouseUp) {
                    clickTrackMouseDown[i] = false;

                    if (insideRect.Contains(e.mousePosition)) {
                        float distSqr = Vector2.SqrMagnitude(e.mousePosition - clickTrackMouseDownPos[i]);
                        if (distSqr <= clickTrackPosDeviation * clickTrackPosDeviation) {
                            //Click performed
                            callClickTrackFullClickDelegates(i, e.mousePosition);
                        }
                        else {
                            //Drag concluded
                            callClickTrackDragEndedDelegates(i, e.mousePosition);
                        }
                    }
                }
            }

            clicksTracked = true;
        }

        /*
         * Wipes current tracking state for mouse clicks and drags, this is useful when an overriding action is performed
         * like a subwindow resize, and you dont want to propagate similar events (click clicking and dragging) back to the
         * subwindow delegates.
         */
        protected void wipeTrackMouseClickAndDragsState() {
            for (int i = 0; i < TRACK_MOUSEBUTTON_COUNT; i++) {
                clickTrackMouseDown[i] = false;
                clickTrackMouseDownPos[i] = Vector2.zero;
            }
        }

        /*
         * This should be called by the sprite editor, so that drags can properly be tracked even when the mouse leaves the extends of the subwindow (do to gui matrixes and clipping)
         * Dependant on trackFullMouseClicks() being called before it, will call it on its own if it hasnt been called yet.
         */
        public void resizeWindow() {
            if (isResizable() && (editor.getMostLikelyFocusedSubwindow() == this || resizing)) {
                //Require clicks being tracked
                if (!clicksTracked) {
                    trackMouseClicksAndDrags();
                }

                Event e = Event.current;

                int mouseButton = getMouseDragButton();

                Rect c = currentRect;   

                //Start resizing
                if (editor.subwindowCanResize() && !resizing && e.type == EventType.MouseDrag && e.button == mouseButton) {
                    for (int i = 0; i < resizeRects.Length; i++) {
                        Rect resizeRect = resizeRects[i];

                        Rect editorRelativeRect = new Rect(c.x + resizeRect.x, c.y + resizeRect.y, resizeRect.width, resizeRect.height);

                        Vector2 editorRelativeClickTrackPos = clickTrackMouseDownPos[mouseButton] + c.position; 

                        if (clickTrackMouseDown[mouseButton] && editorRelativeRect.Contains(editorRelativeClickTrackPos) 
                            && editorRelativeRect.Contains(e.mousePosition)) {
                            Vector2 m = e.mousePosition;

                            resizeAnchor = (ResizeRects) i;

                            switch (resizeAnchor) {
                                case ResizeRects.TopLeft:
                                    forcedMouseCursor = MouseCursor.ResizeUpLeft;
                                    resizeMouseOffset = new Vector2(c.x - m.x, c.y - m.y);
                                    break;
                                case ResizeRects.Top:
                                    forcedMouseCursor = MouseCursor.ResizeVertical;
                                    resizeMouseOffset = new Vector2(0, c.y - m.y);
                                    break;
                                case ResizeRects.TopRight:
                                    forcedMouseCursor = MouseCursor.ResizeUpRight;
                                    resizeMouseOffset = new Vector2(c.x + c.width - m.x, c.y - m.y);
                                    break;
                                case ResizeRects.Left:
                                    forcedMouseCursor = MouseCursor.ResizeHorizontal;
                                    resizeMouseOffset = new Vector2(c.x - m.x, 0);
                                    break;
                                case ResizeRects.Right:
                                    forcedMouseCursor = MouseCursor.ResizeHorizontal;
                                    resizeMouseOffset = new Vector2(c.x + c.width - m.x, 0);
                                    break;
                                case ResizeRects.BotLeft:
                                    forcedMouseCursor = MouseCursor.ResizeUpRight;
                                    resizeMouseOffset = new Vector2(c.x - m.x, c.y + c.height - m.y);
                                    break;
                                case ResizeRects.Bot:
                                    forcedMouseCursor = MouseCursor.ResizeVertical;
                                    resizeMouseOffset = new Vector2(0, c.y + c.height - m.y);
                                    break;
                                case ResizeRects.BotRight:
                                    forcedMouseCursor = MouseCursor.ResizeUpLeft;
                                    resizeMouseOffset = new Vector2(c.x + c.width - m.x, c.y + c.height - m.y);
                                    break;
                            }

                            editor.subwindowResizeAcquire(forcedMouseCursor);
                            resizing = true;

                            break;
                        }
                    }
                }

                //Stop resizing
                if (resizing && e.type == EventType.MouseUp && e.button == mouseButton) {
                    editor.subwindowResizeRelease();
                    resizing = false;
                }

                //Do resizing
                if (resizing && e.type == EventType.MouseDrag) {
                    wipeTrackMouseClickAndDragsState();

                    Vector2 m = e.mousePosition;
                    Vector2 off = resizeMouseOffset;

                    float minWidth = 4 * RESIZE_WIDTH;
                    float minHeight = (4 * RESIZE_HEIGHT) + HEADER_HEIGHT;

                    float cx_max = c.x + c.width; //max x coordinate of currentRect
                    float cy_max = c.y + c.height; //max y coordinate of currentRect

                    float mdx = m.x + off.x; //current mousex offset by offset drag position
                    float mdy = m.y + off.y; //current mousey offset by offset drag position

                    switch (resizeAnchor) {
                        case ResizeRects.TopLeft:
                            setCurrentRect(new Rect(mdx, mdy, Mathf.Max(minWidth, cx_max - mdx), Mathf.Max(minHeight, cy_max - mdy)));
                            break;
                        case ResizeRects.Top:
                            setCurrentRect(new Rect(c.x, mdy, c.width, Mathf.Max(minHeight, cy_max - mdy)));
                            break;
                        case ResizeRects.TopRight:
                            setCurrentRect(new Rect(c.x, mdy, Mathf.Max(minWidth, mdx - c.x), Mathf.Max(minHeight, cy_max - mdy)));
                            break;
                        case ResizeRects.Left:
                            setCurrentRect(new Rect(mdx, c.y, Mathf.Max(minWidth, cx_max - mdx), c.height));
                            break;
                        case ResizeRects.Right:
                            setCurrentRect(new Rect(c.x, c.y, Mathf.Max(minWidth, mdx - c.x), c.height));
                            break;
                        case ResizeRects.BotLeft:
                            setCurrentRect(new Rect(mdx, c.y, Mathf.Max(minWidth, cx_max - mdx), Mathf.Max(minHeight, mdy - c.y)));
                            break;
                        case ResizeRects.Bot:
                            setCurrentRect(new Rect(c.x, c.y, c.width, Mathf.Max(minHeight, mdy - c.y)));
                            break;
                        case ResizeRects.BotRight:
                            setCurrentRect(new Rect(c.x, c.y, Mathf.Max(minWidth, mdx - c.x), Mathf.Max(minHeight, mdy - c.y)));
                            break;
                    }
                }
            }
            else {
                if (resizing) {
                    //Must check resizing before setting to false because we use semaphores for limiting subwindow resizing to one window (semaphore instead of bool for debugging purposes)
                    editor.subwindowResizeRelease();
                    resizing = false;
                }
            }
        }

        /*
         * Changes mouse cursor accordingly, built-in base handles resize cursors
         */
        protected virtual void handleCursors() {
            //Resize cursors
            if (isResizable() && (editor.getMostLikelyFocusedSubwindow() == this || resizing)) {
                if (resizing) {
                    //Already in drag, show forced cursor for smooth display
                    EditorGUIUtility.AddCursorRect(new Rect(float.NegativeInfinity, float.NegativeInfinity, float.PositiveInfinity, float.PositiveInfinity), forcedMouseCursor);
                }
                else {
                    //Not in drag, show potential drag cursor
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.TopLeft], MouseCursor.ResizeUpLeft);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.Top], MouseCursor.ResizeVertical);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.TopRight], MouseCursor.ResizeUpRight);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.Left], MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.Right], MouseCursor.ResizeHorizontal);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.BotLeft], MouseCursor.ResizeUpRight);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.Bot], MouseCursor.ResizeVertical);
                    EditorGUIUtility.AddCursorRect(resizeRects[(int) ResizeRects.BotRight], MouseCursor.ResizeUpLeft);
                }
            }
        }

        /*
         * Recalculates header and resize rects based on the currentRect
         * Should be called anytime currentRect is expected to have changed
         */
        protected void recalculateRects() {
            Rect c = currentRect;

            //Resize
            int w = RESIZE_WIDTH;
            int h = RESIZE_HEIGHT;
            float offx = c.width - w;
            float offy = c.height - h;
            float sw = c.width - (2 * w);
            float sh = c.height - (2 * h);

            if (isResizable()) {
                resizeRects = new Rect[8];
                resizeRects[(int) ResizeRects.TopLeft] = new Rect(0, 0, w, h);
                resizeRects[(int) ResizeRects.Top] = new Rect(w, 0, sw, h);
                resizeRects[(int) ResizeRects.TopRight] = new Rect(offx, 0, w, h);
                resizeRects[(int) ResizeRects.Left] = new Rect(0, h, w, sh);
                resizeRects[(int) ResizeRects.Right] = new Rect(offx, h, w, sh);
                resizeRects[(int) ResizeRects.BotLeft] = new Rect(0, offy, w, h);
                resizeRects[(int) ResizeRects.Bot] = new Rect(w, offy, sw, h);
                resizeRects[(int) ResizeRects.BotRight] = new Rect(offx, offy, w, h);
            }

            //Header
            if (isResizable()) {
                headerRect = new Rect(w, h, sw, HEADER_HEIGHT - h);
            }
            else {
                headerRect = new Rect(0, 0, c.width, HEADER_HEIGHT);
            }
        }

        protected void callClickTrackFullClickDelegates(int mouseButton, Vector2 clickPos) {
            const int MOUSELEFT = 0;
            const int MOUSERIGHT = 1;
            const int MOUSEMIDDLE = 2;

            switch (mouseButton) {
                case MOUSELEFT:
                    if (LeftMouseButtonClickTracked != null) {
                        LeftMouseButtonClickTracked(clickPos);
                    }
                    return;
                case MOUSERIGHT:
                    if (RightMouseButtonClickTracked != null) {
                        RightMouseButtonClickTracked(clickPos);
                    }
                    return;
                case MOUSEMIDDLE:
                    if (MiddleMouseButtonClickTracked != null) {
                        MiddleMouseButtonClickTracked(clickPos);
                    }
                    return;
            }
        }

        protected void callClickTrackDraggedDelegates(int mouseButton, Vector2 dragPos) {
            const int MOUSELEFT = 0;
            const int MOUSERIGHT = 1;
            const int MOUSEMIDDLE = 2;

            switch (mouseButton) {
                case MOUSELEFT:
                    if (LeftMouseButtonDragged != null) {
                        LeftMouseButtonDragged(dragPos);
                    }
                    return;
                case MOUSERIGHT:
                    if (RightMouseButtonDragged != null) {
                        RightMouseButtonDragged(dragPos);
                    }
                    return;
                case MOUSEMIDDLE:
                    if (MiddleMouseButtonDragged != null) {
                        MiddleMouseButtonDragged(dragPos);
                    }
                    return;
            }
        }

        protected void callClickTrackDragEndedDelegates(int mouseButton, Vector2 dragPos) {
            const int MOUSELEFT = 0;
            const int MOUSERIGHT = 1;
            const int MOUSEMIDDLE = 2;

            switch (mouseButton) {
                case MOUSELEFT:
                    if (LeftMouseButtonDragEnded != null) {
                        LeftMouseButtonDragEnded(dragPos);
                    }
                    return;
                case MOUSERIGHT:
                    if (RightMouseButtonDragEnded != null) {
                        RightMouseButtonDragEnded(dragPos);
                    }
                    return;
                case MOUSEMIDDLE:
                    if (MiddleMouseButtonDragEnded != null) {
                        MiddleMouseButtonDragEnded(dragPos);
                    }
                    return;
            }
        }

        /*
         * Restricts this currentRect to fall inside of the other rect
         */
        public void clampInsideRect(Rect other) {
            float x = currentRect.x;
            float y = currentRect.y;
            float w = currentRect.size.x;
            float h = currentRect.size.y;
            Rect minRect = getMinRect();
            float newW = (isResizable()) ? (Mathf.Max(w, Mathf.Min(minRect.width, other.width))) : (w);
            float newH = (isResizable()) ? (Mathf.Max(h, Mathf.Min(minRect.height, other.height))) : (h);

            Rect newRect = new Rect();

            newRect.x = Mathf.Max(other.x, Mathf.Min(x, other.x + other.size.x - newW));
            newRect.y = Mathf.Max(other.y, Mathf.Min(y, other.y + other.size.y - newH));
            newRect.width = newW;
            newRect.height = newH;

            setCurrentRect(newRect);
        }

        public Rect getCurrentRect() {
            return currentRect;
        }

        public void setCurrentRect(Rect rect) {
            currentRect = rect;

            recalculateRects();
        }

        public int getWindowID() {
            return windowID;
        }

        public void setWindowID(int windowID) {
            this.windowID = windowID;
        }

        public virtual string getTitle() {
            return "Subwindow";
        }

        public virtual int getMouseDragButton() {
            return 0;
        }

        public virtual bool isResizable() {
            return false;
        }

        /*
         * Returns true if the left mouse button was just released inside the event, this can be used as a check for button presses
         * so that buttons execute logic only when the left mouse button is released on them.
         *
         * Grabs its own reference to Event.current due to the nature of how buttons process input in the unity GUI, passing an event to it
         * might yield the incorrect event for when the button has actually been clicked, so it must be implemented this way.
         */
        protected virtual bool isGUIButtonClick() {
            Event e = Event.current;

            return (e.button == 0);
        }

        /*
         * The default sizing rect of the window
         */
        public abstract Rect getDefaultRect();

        /*
         * The minimum rect this window should ever be set to, will resize to this if a smaller editor window opens it
         */
        public virtual Rect getMinRect() {
            return getDefaultRect();
        }

        /*
         * Should return the variant name string of a subwindow that is expected to be unique from any other subwindow variants
         * Example, if a subwindow is called "Color Palette", a good identifier would be "colorPalette"
         */
        public virtual string getSubWindowStringIdentifier() {
            return "unnamed";
        }

        public abstract GUIStyle getGUIStyle(GUISkin skin);

        //This is called by the outside parent editorwindow that creates the subwindows, override and call base method to add more save functionality
        public virtual void saveUserSettings() {
            string prefix = FizzikSpriteEditor.EditorPrefs_Prefix;
            string id = getSubWindowStringIdentifier();

            //Save current rect
            EditorPrefs.SetFloat(prefix + id + _rectx, currentRect.x);
            EditorPrefs.SetFloat(prefix + id + _recty, currentRect.y);
            EditorPrefs.SetFloat(prefix + id + _rectw, currentRect.size.x);
            EditorPrefs.SetFloat(prefix + id + _recth, currentRect.size.y);
            EditorPrefs.SetBool(prefix + id + _enabled, enabled);
        }

        /* 
         * This is called inside of the subwindow's base constructor to load any saved settings, 
         * If it is modifying currentRect, it must use setCurrentRect to maintain window functionality
         * Override and call base method to add more load functionality
         */
        public virtual void loadUserSettings() {
            string prefix = FizzikSpriteEditor.EditorPrefs_Prefix;
            string id = getSubWindowStringIdentifier();

            if (isResizable()) {
                setCurrentRect(new Rect(
                    EditorPrefs.GetFloat(prefix + id + _rectx, getDefaultRect().x),
                    EditorPrefs.GetFloat(prefix + id + _recty, getDefaultRect().y),
                    EditorPrefs.GetFloat(prefix + id + _rectw, getDefaultRect().size.x),
                    EditorPrefs.GetFloat(prefix + id + _recth, getDefaultRect().size.y)
                ));
            }
            else {
                setCurrentRect(new Rect(
                    EditorPrefs.GetFloat(prefix + id + _rectx, getDefaultRect().x),
                    EditorPrefs.GetFloat(prefix + id + _recty, getDefaultRect().y),
                    getDefaultRect().size.x, //Fixed default size
                    getDefaultRect().size.y //Fixed default size
                ));
            }
            enabled = EditorPrefs.GetBool(prefix + id + _enabled, enabled);
        }

        //Negate the value that would be returned by isEnabled()
        // Make sure to cancel resizing if it was active
        public void toggleEnabled() {
            if (resizing) {
                editor.subwindowResizeRelease();
                resizing = false;
            }

            enabled = !enabled;
        }

        //Should return whether or not this subwindow is enabled (the subwindow itself shouldnt do anything with this value, editor will handle choosing what to do)
        public bool isEnabled() {
            return enabled;
        }

        //Should return the stored relative window position, this value should have been set by the editor, and will be used by editor
        public Vector2 getRelativeWindowPosition() {
            return relativePos;
        }

        //Should be set only by the editor, as it will be used by the editor to reposition the window
        public void setRelativeWindowPosition(Vector2 relpos) {
            relativePos = relpos;
        }

        public virtual void destroy() { } //Should cleanup any memory such as textures

        /*-------------------------- 
         * Editorprefs constants
         ---------------------------*/
        const string _rectx = "_rectx";
        const string _recty = "_recty";
        const string _rectw = "_rectw";
        const string _recth = "_recth";
        const string _enabled = "_enabled";
    }
}
