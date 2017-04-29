using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;

namespace Fizzik {
    [CreateAssetMenu(fileName = "FizzikSprite", menuName = "Fizzik/Sprite", order = 1)]
    [System.Serializable]
    public class FizzikSprite : ScriptableObject {
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
