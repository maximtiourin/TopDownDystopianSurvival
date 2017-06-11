using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

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

        public int layerNameCount = 1; //Tracks unique layer names history
        public int workingLayer = 0; //Index of current working layer

        public Texture2D texture;

        public string name = "Frame <Unnamed>";

        public FizzikFrame(int w, int h, string name = "Frame <Unnamed>") {
            imgWidth = w;
            imgHeight = h;
            this.name = name;

            layers = new List<FizzikLayer>();
            layers.Add(new FizzikLayer(imgWidth, imgHeight, FizzikLayer.getDefaultLayerName(layerNameCount++))); //Add default first layer

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
                Color[] pixels = null;

                foreach (FizzikLayer layer in visibleLayers) {
                    if (pixels == null) {
                        pixels = FizzikLayer.blend(layer.pixels, layer.opacity, layer.pixels, layer.opacity, layer.blendMode);
                    }
                    else {
                        pixels = FizzikLayer.blend(pixels, 1f, layer.pixels, layer.opacity, layer.blendMode);
                    }
                }

                texture = new Texture2D(imgWidth, imgHeight);
                texture.SetPixels(pixels);
                texture.filterMode = FilterMode.Point;
                texture.Apply();
            }
            else {
                texture = new Texture2D(imgWidth, imgHeight);
                texture.SetPixels(Enumerable.Repeat(Color.clear, imgWidth * imgHeight).ToArray());
                texture.filterMode = FilterMode.Point;
                texture.Apply();
            }
        }

        public FizzikLayer getCurrentLayer() {
            return getLayer(workingLayer);
        }

        public void setCurrentLayer(int layer) {
            workingLayer = layer;
        }

        public FizzikLayer getLayer(int index) {
            if (layers.Count > 0) {
                return layers[Mathf.Clamp(index, 0, layers.Count - 1)];
            }
            else {
                return null;
            }
        }

        /*
         * Adds a brand new layer on top of the currently selected layer, will select the new layer after creation.
         */
        public FizzikLayer createNewLayer(Object undoObject = null) {
            string layerName = FizzikLayer.getDefaultLayerName(layerNameCount++);

            if (undoObject) {
                Undo.RecordObject(undoObject, "Create Layer (" + layerName + ")");
            }

            FizzikLayer layer = new FizzikLayer(imgWidth, imgHeight, layerName);

            layers.Insert(workingLayer + 1, layer);
            workingLayer = workingLayer + 1;

            return layer;
        }

        /*
         * Deletes the currently selected layer, will select the layer below it, or the layer at index 0 otherwise. Will never delete the last existing layer
         */
        public void deleteCurrentLayer(Object undoObject = null) {
            if (layers.Count > 1) {
                if (undoObject) {
                    Undo.RecordObject(undoObject, "Delete Layer (" + layers[workingLayer].name + ")");
                }

                layers.RemoveAt(workingLayer);
                workingLayer = Mathf.Max(0, workingLayer - 1);

                updateTexture();
            }
        }

        public static string getDefaultFrameName(int index) {
            return "Frame " + index;
        }
    }
}
