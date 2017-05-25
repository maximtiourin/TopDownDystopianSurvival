using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    [CustomEditor(typeof(FizzikSprite))]
    public class FizzikSpriteInspector : Editor {

        public override void OnInspectorGUI() {
            FizzikSprite obj = (FizzikSprite) target;

            EditorGUILayout.BeginVertical();

            EditorGUILayout.LabelField("Frame Count: " + obj.frames.Count);
            EditorGUILayout.LabelField("Layer 0 pixeldata:");
            EditorGUILayout.LabelField(obj.getFrame(0).getLayer(0).pixels.ToString());

            EditorGUILayout.EndVertical();
        }
    }
}
