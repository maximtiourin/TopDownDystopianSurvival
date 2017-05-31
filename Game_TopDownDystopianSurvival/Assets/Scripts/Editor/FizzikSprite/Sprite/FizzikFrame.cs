using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Fizzik {
    /*
     * A FizzikFrame is a collection of layers that also stores animation information such as duration, etc.
     * @author Maxim Tiourin
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
         * Called by fizziksprite when the sprite has been loaded from assetpath, and needs to have its textures remade.
         */
        public void reconstructTextures() {
            foreach (FizzikLayer layer in layers) {
                layer.reconstructTexture();
            }

            updateTexture();
        }

        public void destroyTextures() {
            foreach (FizzikLayer layer in layers) {
                layer.destroyTexture();
            }

            if (texture) {
                Object.DestroyImmediate(texture);
            }
        }

        /*
         * Gets the finalized texture that contains all visible layers and combines them using
         * their appropriate blend modes
         */
        public void updateTexture() {
            List<FizzikLayer> visibleLayers = layers.FindAll((layer) => { return layer.visible; });

            //Wipe texture
            if (texture != null) Object.DestroyImmediate(texture);
            texture = null;

            if (visibleLayers.Count > 0) {
                foreach (FizzikLayer layer in visibleLayers) {
                    //if (layer.texture) {
                        if (texture == null) {
                            texture = FizzikLayer.blend(layer.texture, layer.opacity, layer.texture, layer.opacity, layer.blendMode);
                        }
                        else {
                            texture = FizzikLayer.blend(texture, 1f, layer.texture, layer.opacity, layer.blendMode);
                        }
                    //}
                }
            }
            else {
                texture = new Texture2D(imgWidth, imgHeight);
                texture.SetPixels(Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray());
                texture.filterMode = FilterMode.Point;
                texture.Apply();
            }
        }

        public FizzikLayer getLayer(int index) {
            if (layers.Count > 0) {
                return layers[Mathf.Clamp(index, 0, layers.Count - 1)];
            }
            else {
                return null;
            }
        }
    }
}
