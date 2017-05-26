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
        public float opacity; //Internal pixel data is independent of layer opacity. Layer opacity dictates how a frame blends layers together, as well as how the texture is drawn in preview.
        public bool visible;
        public bool locked;

        public Texture2D texture;

        public FizzikLayer(int w, int h) {
            imgWidth = w;
            imgHeight = h;

            pixels = Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray();

            blendMode = BlendMode.NORMAL;
            opacity = 1f;
            visible = true;
            locked = false;

            reconstructTexture();
        }

        /*
         * Called when the texture needs to be completely remade from pixel array data
         */
        public void reconstructTexture() {
            texture = new Texture2D(imgWidth, imgHeight);
            texture.filterMode = FilterMode.Point;

            updateTexture();
        }

        public void destroyTexture() {
            if (texture) {
                Object.DestroyImmediate(texture);
            }
        }

        /*
         * Updates the layer's internal texture using it's pixel data
         */
        public void updateTexture() {
            texture.SetPixels(pixels);
            texture.Apply();
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

        /*
         * Sets pixel at x,y with origin at bottom left, coords increasing top right
         * This is in the format that Texture2D handles pixels
         * If batch = true, won't alter underlying texture, just the pixel data structure.
         */
        public void setPixel(int x, int y, Color color, bool batch = false) {
            int px = Mathf.Clamp(x, 0, imgWidth - 1);
            int py = Mathf.Clamp(y, 0, imgHeight - 1);
            pixels[(py * imgWidth + px)] = color;

            if (!batch) {
                //Alter layer texture
                texture.SetPixel(px, py, color);
                texture.Apply();
            }
        }

        public void setPixelTopLeftOrigin(int x, int y, Color color, bool batch = false) {
            int px = Mathf.Clamp(x, 0, imgWidth - 1);
            int py = Mathf.Clamp(y, 0, imgHeight - 1);
            pixels[((imgHeight - 1 - py) * imgWidth) + px] = color;

            if (!batch) {
                //Alter layer texture
                texture.SetPixel(px, imgHeight - 1 - py, color);
                texture.Apply();
            }
        }

        public static Texture2D blend(Texture2D lowerTex, float lowerOpacity, Texture2D upperTex, float upperOpacity, BlendMode blendMode) {
            switch (blendMode) {
                case BlendMode.NORMAL:
                    return blendNormal(lowerTex, lowerOpacity, upperTex, upperOpacity);
                default:
                    return blendNormal(lowerTex, lowerOpacity, upperTex, upperOpacity);
            }
        }

        private static Texture2D blendNormal(Texture2D lowerTex, float lowerOpacity, Texture2D upperTex, float upperOpacity) {
            Texture2D res = new Texture2D(upperTex.width, upperTex.height);
            res.filterMode = upperTex.filterMode;

            for (int x = 0; x < upperTex.width; x++) {
                for (int y = 0; y < upperTex.height; y++) {
                    Color lowerPixel = lowerTex.GetPixel(x, y);
                    Color upperPixel = upperTex.GetPixel(x, y);

                    float lowerAlpha = lowerPixel.a * lowerOpacity;
                    float upperAlpha = upperPixel.a * upperOpacity;
                    float resAlpha = upperAlpha + (lowerAlpha * (1f - upperAlpha));

                    Color resPixel = new Color(((lowerPixel.r * lowerAlpha * (1f - upperAlpha)) + (upperPixel.r * upperAlpha)) / resAlpha,
                                               ((lowerPixel.g * lowerAlpha * (1f - upperAlpha)) + (upperPixel.g * upperAlpha)) / resAlpha,
                                               ((lowerPixel.b * lowerAlpha * (1f - upperAlpha)) + (upperPixel.b * upperAlpha)) / resAlpha,
                                               resAlpha);

                    res.SetPixel(x, y, resPixel);
                }
            }

            res.Apply();

            return res;
        }
    }
}
