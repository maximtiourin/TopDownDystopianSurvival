using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    /*
     * A FizzikFrame is a collection of layers that also stores animation information such as duration, etc.
     */
    [System.Serializable]
    public class FizzikFrame {
        public int imgWidth;
        public int imgHeight;

        public List<FizzikLayer> layers;

        public Texture2D texture;

        public FizzikFrame(int w, int h) {
            imgWidth = w;
            imgHeight = h;

            layers = new List<FizzikLayer>();
            layers.Add(new FizzikLayer(imgWidth, imgHeight)); //Add default first layer

            texture = new Texture2D(imgWidth, imgHeight);
            texture.filterMode = FilterMode.Point;

            updateTexture();
        }

        /*
         * Gets the finalized texture that contains all visible layers and combines them using
         * their appropriate blend modes
         *
         * TODO - implement blend modes (Look into efficient ways of blending arrays of pixels, so that im not looping over too many when setting texture)
         */
        public void updateTexture() {
            bool firstVisibleLayer = true;
            foreach (FizzikLayer layer in layers) {
                if (layer.visible) {
                    //TODO DEBUG - should take blendmodes into account, for now just 
                    if (firstVisibleLayer) {
                        //Set the base pixels
                        texture.SetPixels(layer.pixels);

                        firstVisibleLayer = false;
                    }
                    else {
                        //Start blending pixels together
                    }
                }
            }

            texture.Apply();
        }
    }
}
