using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Fizzik {
    /*
     * A FizzikSprite is a collection of FizzikFrames that is capable of running through and manipulating those frames
     * in a variety of ways.
     * @author Maxim Tiourin
     */
    [System.Serializable]
    public class FizzikSprite : ScriptableObject {
        public int imgWidth;
        public int imgHeight;

        public List<FizzikFrame> frames;

        const int RECENT_COLORS_SIZE = 10;
        public Color[] recentColors; //Tracks recently selected colors when this sprite was edited

        public bool hasInit = false;

        public void Init(int w, int h) {
            if (!hasInit) {
                imgWidth = w;
                imgHeight = h;

                frames = new List<FizzikFrame>();
                frames.Add(new FizzikFrame(imgWidth, imgHeight)); //Add the default first frame

                recentColors = Enumerable.Repeat(Color.clear, RECENT_COLORS_SIZE).ToArray();
                recentColors[0] = Color.white;
                recentColors[1] = Color.black;

                hasInit = true;
            }
        }

        /*
         * Called when the sprite has been loaded from assetpath, and needs to have its textures remade.
         */
        public void reconstructTextures() {
            foreach (FizzikFrame frame in frames) {
                frame.reconstructTextures();
            }
        }

        public void destroyTextures() {
            foreach (FizzikFrame frame in frames) {
                frame.destroyTextures();
            }
        }

        public FizzikFrame getFrame(int index) {
            if (frames.Count > 0) {
                return frames[Mathf.Clamp(index, 0, frames.Count - 1)];
            }
            else {
                return null;
            }
        }

        public Texture2D getTextureFromFrame(int index) {
            FizzikFrame frame = getFrame(index);

            if (frame != null) {
                return frame.texture;
            }
            else {
                return null;
            }
        }

        public void offerRecentColor(Color color) {
            if (recentColors[0].Equals(color)) {
                return;
            }

            int index = hasRecentColor(color);
            if (index > 0) {
                //Swap recent color
                swapRecentColor(index);
            }
            else if (index < 0) {
                //Push recent color;
                pushRecentColor(color);
            }
        }

        private void swapRecentColor(int index) {
            Color color = recentColors[index];

            Color[] arr = new Color[recentColors.Length];

            for (int i = 0; i < index; i++) {
                arr[i + 1] = recentColors[i];
            }

            for (int i = index + 1; i < arr.Length; i++) {
                arr[i] = recentColors[i];
            }

            arr[0] = color;

            recentColors = arr;
        }

        /*
         * Pushes the unique recent color onto the stack
         */
        private void pushRecentColor(Color color) {
            Color[] arr = new Color[recentColors.Length];

            arr[0] = color;
            for (int i = 1; i < arr.Length; i++) {
                arr[i] = recentColors[i - 1];
            }
            recentColors = arr;
        }

        /*
         * Returns index of color if the recentColors stack has the given color,
         * otherwise returns -1;
         */
        private int hasRecentColor(Color color) {
            for (int i = 0; i < recentColors.Length; i++) {
                if (recentColors[i].Equals(color)) {
                    return i;
                }
            }

            return -1;
        }


        [OnOpenAsset(1)]
        public static bool openFromProjectBrowser(int instanceID, int line) {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is FizzikSprite) {
                return FizzikSpriteEditor.openAssetFromProjectBrowser(obj);
            }

            return false;
        }
    }
}
