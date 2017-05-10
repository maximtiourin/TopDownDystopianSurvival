using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Fizzik {
    /*
     * A FizzikSprite is a collection of FizzikFrames that is capable of running through and manipulating those frames
     * in a variety of ways.
     */
    //[CreateAssetMenu(fileName = "FizzikSprite", menuName = "Fizzik/Sprite", order = 1)] //Cant use this because we need to be able to init width and height
    [System.Serializable]
    public class FizzikSprite : ScriptableObject {
        public int imgWidth;
        public int imgHeight;

        public List<FizzikFrame> frames;

        public bool hasInit = false;

        public void Init(int w, int h) {
            if (!hasInit) {
                imgWidth = w;
                imgHeight = h;

                frames = new List<FizzikFrame>();
                frames.Add(new FizzikFrame(imgWidth, imgHeight)); //Add the default first frame

                hasInit = true;
            }
        }

        public Texture2D getTextureFromFrame(int index) {
            if (frames.Count > 0) {
                return frames[Mathf.Clamp(index, 0, frames.Count - 1)].texture;
            }
            else {
                return null;
            }
        }




        [OnOpenAsset(1)]
        public static bool openFromProjectBrowser(int instanceID, int line) {
            Object obj = EditorUtility.InstanceIDToObject(instanceID);

            if (obj is FizzikSprite) {
                return FizzikSpriteEditor.openAssetFromProjectBrowser(EditorUtility.InstanceIDToObject(instanceID));
            }

            return false;
        }
    }
}
