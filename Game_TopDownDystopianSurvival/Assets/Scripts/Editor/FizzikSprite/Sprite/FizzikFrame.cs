using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    /*
     * A FizzikFrame is a collection of layers that also stores animation information such as duration, etc.
     */
    [System.Serializable]
    public class FizzikFrame {
        public FizzikSprite sprite; //Parent sprite

        public List<FizzikLayer> layers;

        public FizzikFrame(FizzikSprite sprite) {
            this.sprite = sprite;

            layers = new List<FizzikLayer>();
            layers.Add(new FizzikLayer(this)); //Add default first layer
        }
    }
}
