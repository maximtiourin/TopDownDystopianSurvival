using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class CreateNewSpriteOptions : FizzikMenuOptionsWindow {
        int pw = 64;
        int ph = 64;

        public override void Init(FizzikSpriteEditor editor) {
            base.Init(editor);

            float w = dss_CreateNewSpriteOptions_rect.size.x;
            float h = dss_CreateNewSpriteOptions_rect.size.y;
            float hw = w / 2f;
            float hh = h / 2f;

            position = new Rect(editor.position.center.x - hw, editor.position.center.y - hh, w, h);
            minSize = new Vector2(w, h);
            maxSize = new Vector2(w, h);
        }

        void OnGUI() {
            titleContent = new GUIContent("Create New Sprite Options");

            EditorGUILayout.BeginVertical();

            pw = Mathf.Max(1, EditorGUILayout.IntField("Pixel Width", pw));
            ph = Mathf.Max(1, EditorGUILayout.IntField("Pixel Height", ph));

            GUILayout.Space(32f);

            if (GUILayout.Button(new GUIContent("Create"))) {
                //Call create new sprite from editor with options
                editor.createNewSprite(pw, ph);

                closeWindow();
            }

            EditorGUILayout.EndVertical();
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_CreateNewSpriteOptions_rect = new Rect(0, 0, 240, 100);
    }
}
