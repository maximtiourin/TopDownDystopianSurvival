using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class RectUtility : MonoBehaviour {
        public static Vector2 topLeft(Rect rect) {
            return new Vector2(rect.xMin, rect.yMin);
        }

        public static Rect scaleBy(Rect rect, float scale) {
            return scaleBy(rect, scale, rect.center);
        }

        public static Rect scaleBy(Rect rect, float scale, Vector2 pivot) {
            Rect r = rect;
            
            r.x -= pivot.x;
            r.y -= pivot.y;
            r.xMin *= scale;
            r.yMin *= scale;
            r.xMax *= scale;
            r.yMax *= scale;
            r.x += pivot.x;
            r.y += pivot.y;

            return r;
        }

        public static Rect scaleBy(Rect rect, Vector2 scale) {
            return scaleBy(rect, scale, rect.center);
        }

        public static Rect scaleBy(Rect rect, Vector2 scale, Vector2 pivot) {
            Rect r = rect;

            r.x -= pivot.x;
            r.y -= pivot.y;
            r.xMin *= scale.x;
            r.yMin *= scale.y;
            r.xMax *= scale.x;
            r.yMax *= scale.y;
            r.x += pivot.x;
            r.y += pivot.y;

            return r;
        }
    }
}
