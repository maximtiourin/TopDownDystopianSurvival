using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public abstract class FizzikSubWindow {
        const int HEADER_HEIGHT = 15;
        const int RESIZE_WIDTH = 8;
        const int RESIZE_HEIGHT = 8;

        protected FizzikSpriteEditor editor;
        protected Vector2 relativePos = Vector2.zero;
        protected Rect currentRect; //The bounds of the window
        protected Rect headerRect; //The bounds of the window header
        protected Rect resizeRect; //The bounds of the area where window resizing activates
        protected int windowID;
        protected bool enabled = true;

        protected bool resizing;
        protected Vector2 resizeMouseOffset;

        public FizzikSubWindow(FizzikSpriteEditor editor) {
            this.editor = editor;

            loadUserSettings();
        }

        public abstract void handleGUI(int windowID);

        protected void dragWindow() {
            Event e = Event.current;

            if (e.button == getMouseDragButton()) {
                GUI.DragWindow(headerRect);
            }
        }

        /*
         * This should be called by the sprite editor, so that drags can properly be tracked even when the mouse leaves the extends of the subwindow (do to gui matrixes and clipping)
         */
        public void resizeWindow() {
            if (isResizable()) {
                Event e = Event.current;

                int mouseButton = getMouseDragButton();

                Rect c = currentRect;

                Rect editorRelativeRect = new Rect(c.x + resizeRect.x, c.y + resizeRect.y, resizeRect.width, resizeRect.height);

                //Start resizing
                if (!resizing && e.type == EventType.MouseDrag && e.button == mouseButton) {
                    if (editorRelativeRect.Contains(e.mousePosition)) {
                        Vector2 m = e.mousePosition;

                        resizing = true;
                        resizeMouseOffset = new Vector2(c.x + c.width - m.x, c.y + c.height - m.y);
                    }
                }

                //Stop resizing
                if (resizing && e.type == EventType.MouseUp && e.button == mouseButton) {
                    resizing = false;
                }

                //Do resizing
                if (resizing && e.type == EventType.MouseDrag) {
                    Vector2 m = e.mousePosition;
                    Vector2 off = resizeMouseOffset;

                    Rect newRect = new Rect(c.x, c.y, Mathf.Max(4 * RESIZE_WIDTH, m.x - c.x + off.x), Mathf.Max((4 * RESIZE_HEIGHT) + HEADER_HEIGHT, m.y - c.y + off.y));

                    setCurrentRect(newRect);
                }
            }
            else {
                resizing = false;
            }
        }

        /*
         * Changes mouse cursor accordingly, built-in base handles resize cursors
         */
        protected virtual void handleCursors() {
            EditorGUIUtility.AddCursorRect(resizeRect, MouseCursor.ResizeUpLeft);
        }

        /*
         * Recalculates header and resize rects based on the currentRect
         * Should be called anytime currentRect is expected to have changed
         */
        protected void recalculateRects() {
            Rect c = currentRect;

            //Header
            headerRect = new Rect(0, 0, c.width, HEADER_HEIGHT);

            //Resize
            resizeRect = new Rect(c.width - RESIZE_WIDTH, c.height - RESIZE_HEIGHT, RESIZE_WIDTH, RESIZE_HEIGHT);
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
         * The default sizing rect of the window
         */
        public abstract Rect getDefaultRect();

        /*
         * The minimum rect this window should ever be set to, will resize to this if a smaller editor window opens it
         */
        public virtual Rect getMinRect() {
            return getDefaultRect();
        }

        public abstract GUIStyle getGUIStyle(GUISkin skin);

        //This is called by the outside parent editorwindow that creates the subwindows
        public abstract void saveUserSettings();

        //This should be called inside of the subwindow's constructor if it wants to use any saved settings
        public abstract void loadUserSettings();

        //Negate the value that would be returned by isEnabled()
        public void toggleEnabled() {
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
    }
}
