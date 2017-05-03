using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    /*
     * A FizzikLayer stores pixel color information, as well as things such as blending modes and layer opacity,
     * whether or not the layer is locked or visible, etc.
     */
    [System.Serializable]
    public class FizzikLayer {
        public enum BlendMode {
            NORMAL
        }

        public FizzikFrame frame; //Parent frame

        public Color[] pixels; //2D array flattened to 1D (y * width + x)
        public BlendMode blendMode; //How the pixels of this layer should interact with layer pixels below this one
        public bool visible;
        public bool locked;

        public FizzikLayer(FizzikFrame frame) {
            this.frame = frame;

            pixels = new Color[frame.sprite.imgWidth * frame.sprite.imgHeight];

            blendMode = BlendMode.NORMAL;
            visible = true;
            locked = false;
        }
    }
}
