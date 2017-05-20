using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

            updateTexture(); //Texture is initialized in here
        }

        /*
         * Gets the finalized texture that contains all visible layers and combines them using
         * their appropriate blend modes
         */
        public void updateTexture() {
            List<FizzikLayer> visibleLayers = layers.FindAll((layer) => { return layer.visible = true; });

            //Wipe texture
            Object.DestroyImmediate(texture);
            texture = null;

            if (visibleLayers.Count > 0) {
                foreach (FizzikLayer layer in visibleLayers) {
                    if (texture == null) {
                        texture = FizzikLayer.blend(layer.texture, layer.opacity, layer.texture, layer.opacity, layer.blendMode);
                    }
                    else {
                        texture = FizzikLayer.blend(texture, 1f, layer.texture, layer.opacity, layer.blendMode);
                    }
                }
            }
            else {
                texture = new Texture2D(imgWidth, imgHeight);
                texture.SetPixels(Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray());
                texture.filterMode = FilterMode.Point;
                texture.Apply();
            }
        }
    }
}
