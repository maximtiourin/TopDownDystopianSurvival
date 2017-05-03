using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    public class Texture2DUtility : MonoBehaviour {
        /*
         * Sets the pixel of the texture, takes coordinates as if the texture's origin was at the top left,
         * whereas Texture2D.SetPixel treats the origin as the bottom left. GUI windows treat origin as top left,
         * so this is useful when handling textures inside of those contexts.
         */
        public static void SetPixel(Texture2D tex, int x, int y, Color color) {
            int h = tex.height;

            tex.SetPixel(x, h - 1 - y, color);
        }
    }
}
