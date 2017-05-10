using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Fizzik {
    /*
     * A FizzikLayer stores pixel color information, as well as things such as blending modes and layer opacity,
     * whether or not the layer is locked or visible, as well as methods to aid in 'drawing' pixels inside the layer, etc.
     */
    [System.Serializable]
    public class FizzikLayer {
        public enum BlendMode {
            NORMAL
        }

        public int imgWidth;
        public int imgHeight;

        public Color[] pixels; //2D array flattened to 1D (y * width + x)
        public BlendMode blendMode; //How the pixels of this layer should interact with layer pixels below this one
        public float opacity;
        public bool visible;
        public bool locked;

        public FizzikLayer(int w, int h) {
            imgWidth = w;
            imgHeight = h;

            pixels = Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray();

            blendMode = BlendMode.NORMAL;
            opacity = 1f;
            visible = true;
            locked = false;
        }

        /*
         * Returns pixel at x,y with origin at bottom left, coords increasing top right
         * This is in the format that Texture2D handles pixels
         */
        public Color getPixel(int x, int y) {
            int px = Mathf.Clamp(x, 0, imgWidth - 1);
            int py = Mathf.Clamp(y, 0, imgHeight - 1);
            return pixels[(py * imgWidth + px)];
        }

        /*
         * Returns pixel at x,y with origin top left, coords increasing bottom right
         * This uses origin similar to unity GUI, which can make things easier when working
         * in those contexts
         */
        public Color getPixelTopLeftOrigin(int x, int y) {
            int px = Mathf.Clamp(x, 0, imgWidth - 1);
            int py = Mathf.Clamp(y, 0, imgHeight - 1);
            return pixels[((imgHeight - 1 - py) * imgWidth) + px];
        }
    }
}
