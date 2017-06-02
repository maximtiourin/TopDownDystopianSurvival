using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Fizzik {
    /*
     * A FizzikLayer stores pixel color information, as well as things such as blending modes and layer opacity,
     * whether or not the layer is locked or visible, as well as methods to aid in 'drawing' pixels inside the layer, etc.
     *
     * @author - Maxim Tiourin
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

        //Batch drawing optimizations
        private bool batched = false;
        private Vector2 minBatchPoint;
        private Vector2 maxBatchPoint;

        public FizzikLayer(int w, int h) {
            imgWidth = w;
            imgHeight = h;

            pixels = Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray();

            blendMode = BlendMode.NORMAL;
            opacity = 1f;
            visible = true;
            locked = false;

            clearBatch();

            reconstructTexture();
        }

        /*
         * Called when the texture needs to be completely remade from pixel array data
         */
        public void reconstructTexture() {
            destroyTexture();

            texture = new Texture2D(imgWidth, imgHeight);
            texture.filterMode = FilterMode.Point;

            updateTexture(true);
        }

        /*
         * Destroys underlying layer texture while preserving pixel array data
         */
        public void destroyTexture() {
            if (texture) {
                Object.DestroyImmediate(texture);
            }

            clearBatch();
        }

        /*
         * Updates the layer's internal texture using its pixel data
         */
        public void updateTexture(bool reconstruct = false) {
            if (texture != null) {
                if (reconstruct) {
                    clearBatch();
                }

                if (!reconstruct && batched) {
                    Vector2 min = minBatchPoint;
                    Vector2 max = maxBatchPoint;

                    Color[] batchArea = getPixelsInArea((int) min.x, (int) min.y, (int) max.x, (int) max.y);

                    texture.SetPixels((int) min.x, (int) min.y, (int) (max.x - min.x + 1), (int) (max.y - min.y + 1), batchArea);

                    clearBatch();
                }
                else {
                    texture.SetPixels(pixels);
                }

                texture.Apply();
            }
        }

        /*
         * Resets batching area points and flag
         */
        public void clearBatch() {
            minBatchPoint = new Vector2(imgWidth - 1, imgHeight - 1);
            maxBatchPoint = Vector2.zero;
            batched = false;
        }

        public void batchPixel(int px, int py) {
            Vector2 min = minBatchPoint;
            Vector2 max = maxBatchPoint;
            minBatchPoint = new Vector2(Mathf.Min(min.x, px), Mathf.Min(min.y, py));
            maxBatchPoint = new Vector2(Mathf.Max(max.x, px), Mathf.Max(max.y, py));

            batched = true;
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

        public Color[] getPixelsInArea(int minx, int miny, int maxx, int maxy) {
            int subwidth = maxx - minx + 1;
            int subheight = maxy - miny + 1;

            Color[] subarr = new Color[subwidth * subheight];

            for (int y = miny; y <= maxy; y++) {
                int suby = y - miny;
                for (int x = minx; x <= maxx; x++) {
                    int subx = x - minx;

                    subarr[suby * subwidth + subx] = pixels[y * imgWidth + x];
                }
            }

            return subarr;
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

            if (batch) {
                batchPixel(px, py);
            }
            else {
                //Alter layer texture
                texture.SetPixel(px, py, color);
                texture.Apply();
            }
        }

        /*
         * Sets pixel at x,y with origin at top left, coords increasing bottom right
         * This uses origin similar to unity GUI, which can make things easier when working
         * in those contexts.
         * If batch = true, won't alter underlying texture, just the pixel data structure.
         */
        public void setPixelTopLeftOrigin(int x, int y, Color color, bool batch = false) {
            int px = Mathf.Clamp(x, 0, imgWidth - 1);
            int py = Mathf.Clamp(y, 0, imgHeight - 1);
            pixels[((imgHeight - 1 - py) * imgWidth) + px] = color;

            if (batch) {
                batchPixel(px, imgHeight - 1 - py);
            }
            else {
                //Alter layer texture
                texture.SetPixel(px, imgHeight - 1 - py, color);
                texture.Apply();
            }
        }

        /*
         * Sets pixels on a linearly interpolated line from 'start' to 'end' where origin is at bottom left, coords increasing top right
         * This is in the format that Texture2D handles pixels
         * If batch = true, won't alter underlying texture, just the pixel data structure.
         */
        public void setPixelsInterpolate(int startx, int starty, int endx, int endy, Color color, bool batch = false) {
            Vector2 start = new Vector2(startx, starty);
            Vector2 end = new Vector2(endx, endy);

            float incAmount = Vector2.Distance(start, end) * 3f;
            float inc = 1f / incAmount;
            
            for (float t = 0f; t <= 1f; t += inc) {
                Vector2 step = Vector2.Lerp(start, end, t);

                setPixel((int) step.x, (int) step.y, color, true); //Internal mini-batch
            }
            
            //If !batch then update our internal mini-batch, otherwise allow other external batching operations to continue
            if (!batch) {
                updateTexture();
            }
        }

        /*
         * Sets pixels on a linearly interpolated line from 'start' to 'end' where origin is at top left, coords increasing bottom right
         * This uses origin similar to unity GUI, which can make things easier when working
         * in those contexts.
         * If batch = true, won't alter underlying texture, just the pixel data structure.
         */
        public void setPixelsInterpolateTopLeftOrigin(int startx, int starty, int endx, int endy, Color color, bool batch = false) {
            Vector2 start = new Vector2(startx, starty);
            Vector2 end = new Vector2(endx, endy);

            float incAmount = Vector2.Distance(start, end) * 3f;
            float inc = 1f / incAmount;

            for (float t = 0f; t <= 1f; t += inc) {
                Vector2 step = Vector2.Lerp(start, end, t);

                setPixelTopLeftOrigin((int) step.x, (int) step.y, color, true); //Internal mini-batch
            }

            //If !batch then update our internal mini-batch, otherwise allow other external batching operations to continue
            if (!batch) {
                updateTexture();
            }
        }

        public static Color[] blend(Color[] lowerPixels, float lowerOpacity, Color[] upperPixels, float upperOpacity, BlendMode blendMode) {
            switch (blendMode) {
                case BlendMode.NORMAL:
                    return blendNormal(lowerPixels, lowerOpacity, upperPixels, upperOpacity);
                default:
                    return blendNormal(lowerPixels, lowerOpacity, upperPixels, upperOpacity);
            }
        }

        /*
         * Blend two pixel data arrays together using the NORMAL blend mode
         */
        private static Color[] blendNormal(Color[] lowerPixels, float lowerOpacity, Color[] upperPixels, float upperOpacity) {
            Color[] res = new Color[upperPixels.Length];

            for (int p = 0; p < upperPixels.Length; p++) {
                Color lowerPixel = lowerPixels[p];
                Color upperPixel = upperPixels[p];

                float lowerAlpha = lowerPixel.a * lowerOpacity;
                float upperAlpha = upperPixel.a * upperOpacity;
                float resAlpha = upperAlpha + (lowerAlpha * (1f - upperAlpha));

                Color resPixel = new Color(((lowerPixel.r * lowerAlpha * (1f - upperAlpha)) + (upperPixel.r * upperAlpha)) / resAlpha,
                                            ((lowerPixel.g * lowerAlpha * (1f - upperAlpha)) + (upperPixel.g * upperAlpha)) / resAlpha,
                                            ((lowerPixel.b * lowerAlpha * (1f - upperAlpha)) + (upperPixel.b * upperAlpha)) / resAlpha,
                                            resAlpha);

                res[p] = resPixel;
            }

            return res;
        }
    }
}
