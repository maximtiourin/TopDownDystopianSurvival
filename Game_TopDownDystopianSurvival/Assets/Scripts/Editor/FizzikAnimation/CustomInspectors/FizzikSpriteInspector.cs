using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    [CustomEditor(typeof(FizzikSprite))]
    public class FizzikSpriteInspector : Editor {

        public override void OnInspectorGUI() {
            EditorGUILayout.LabelField("Testing custom inspector!");
        }
    }
}
