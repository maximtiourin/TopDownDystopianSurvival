using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Fizzik {
    public class GridOverlayOptions : FizzikMenuOptionsWindow {
        public override void Init(FizzikSpriteEditor editor) {
            base.Init(editor);

            float w = dss_GridOverlayOptions_rect.size.x;
            float h = dss_GridOverlayOptions_rect.size.y;
            float hw = w / 2f;
            float hh = h / 2f;

            position = new Rect(editor.position.center.x - hw, editor.position.center.y - hh, w, h);
            minSize = new Vector2(w, h);
            maxSize = new Vector2(w, h);
        }

        void OnGUI() {
            titleContent = new GUIContent("Grid Options");

            EditorGUILayout.BeginVertical();

            editor.setGridOverlayCellWidth(EditorGUILayout.DelayedIntField("Cell Width", editor.getGridOverlayCellWidth()));
            editor.setGridOverlayCellHeight(EditorGUILayout.DelayedIntField("Cell Height", editor.getGridOverlayCellHeight()));
            editor.setGridOverlayColor(EditorGUILayout.ColorField(new GUIContent("Line Color"), editor.getGridOverlayColor(), true, true, false, null));
            editor.setGridOverlayEnabled(EditorGUILayout.Toggle("Enabled", editor.getGridOverlayEnabled()));

            EditorGUILayout.EndVertical();
        }

        /*-------------------------- 
         * Default sizing structures
         ---------------------------*/
        public static Rect dss_GridOverlayOptions_rect = new Rect(0, 0, 400, 100);
    }
}
