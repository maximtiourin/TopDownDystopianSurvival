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

            //Select image dimensions
            pw = Mathf.Max(1, EditorGUILayout.IntField("Image Width", pw));
            ph = Mathf.Max(1, EditorGUILayout.IntField("Image Height", ph));

            //Image Size caveat blurb
            if (pw * ph > 256 * 256) {
                GUIStyle blurbStyle = new GUIStyle(GUI.skin.label);
                blurbStyle.wordWrap = true;
                blurbStyle.fontSize = 9;
                blurbStyle.normal.textColor = ColorUtility.darker(Color.red);

                GUILayout.Label(txt_blurb_sizecaveats, blurbStyle);
            }

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
        public static Rect dss_CreateNewSpriteOptions_rect = new Rect(0, 0, 420, 400); //width::240, height :: 100

        /*-------------------------- 
         * Text constants
         ---------------------------*/
        const string txt_blurb_sizecaveats = "Due to Texture2D limitations, image sizes over 256 x 256 will increasingly suffer image-editing related performance degradations. For a smooth image-editing experience with image sizes of 512x512 and larger, it is better to use an external image editor, and then import the images back in for animating.";
    }
}
