using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class GUIUtility {
        public static void DrawRectangle(Rect rect, Color color, bool outline = true, Texture2D premadePixelTexture = null) {
            Texture2D pixel = premadePixelTexture;

            if (!pixel) {
                pixel = new Texture2D(1, 1);
                pixel.filterMode = FilterMode.Point;
                pixel.SetPixel(0, 0, Color.white);
            }

            Color oldColor = GUI.color;
            GUI.color = color;

            int rx = (int) rect.x;
            int ry = (int) rect.y;
            int rw = (int) rect.width;
            int rh = (int) rect.height;

            if (outline) {
                for (int y = ry; y < ry + rh; y++) {
                    for (int x = rx; x < rx + rw; x++) {
                        if (x == rx || x == rx + rw - 1 || y == ry || y == ry + rh - 1) {
                            GUI.DrawTexture(new Rect(x, y, 1, 1), pixel);
                        }
                    }
                }
            }
            else {
                GUI.DrawTexture(new Rect(rx, ry, rw, rh), pixel);
            }

            Object.DestroyImmediate(pixel);

            GUI.color = oldColor;
        }
    }
}
