using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fizzik {
    /*
     * Facilitates the buffering of pixel drawing intents inside of the sprite editor
     *
     * @author - Maxim Tiourin
     */
    public class DrawPixelIntent {
        public enum IntentType {
            Normal, Interpolate
        }

        public IntentType type;
        public FizzikFrame frame;
        public FizzikLayer layer;
        public int x;
        public int y;
        public int prevx;
        public int prevy;
        public Color color;

        public DrawPixelIntent(IntentType type, FizzikFrame frame, FizzikLayer layer, int px, int py, Color color) {
            this.type = type;
            this.frame = frame;
            this.layer = layer;
            this.x = px;
            this.y = py;
            this.color = color;
        }

        public DrawPixelIntent(IntentType type, FizzikFrame frame, FizzikLayer layer, int prevpx, int prevpy, int px, int py, Color color) {
            this.type = type;
            this.frame = frame;
            this.layer = layer;
            this.prevx = prevpx;
            this.prevy = prevpy;
            this.x = px;
            this.y = py;
            this.color = color;
        }
    }
}
