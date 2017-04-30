using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    /*
     * A ResizeEvent is passed two rects that are not equal, and then calculates and stores the type of resize event that occured.
     */
    public class ResizeEvent {
        public enum ResizeEventType {
            None, Move, Resize, MoveAndResize
        }

        public ResizeEventType type;
        
        public ResizeEvent(Rect oldRect, Rect newRect) {
            Rect r1 = oldRect;
            Rect r2 = newRect;

            bool move = false;
            bool resize = false;

            if (r1.x != r2.x || r1.y != r2.y) {
                move = true;
            }

            if (r1.size != r2.size) {
                resize = true;
            }

            
            if (move && resize) {
                type = ResizeEventType.MoveAndResize;
            }
            else if (move) {
                type = ResizeEventType.Move;
            }
            else if (resize) {
                type = ResizeEventType.Resize;
            }
            else {
                type = ResizeEventType.None;
            }
        }
    }
}
