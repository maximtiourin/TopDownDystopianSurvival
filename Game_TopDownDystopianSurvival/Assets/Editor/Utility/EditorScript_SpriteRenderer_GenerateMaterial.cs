using UnityEngine;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(Script_SpriteRenderer_GenerateMaterial))]
public class EditorScript_SpriteRenderer_GenerateMaterial : Editor {

	public override void OnInspectorGUI() {
		Script_SpriteRenderer_GenerateMaterial script = (Script_SpriteRenderer_GenerateMaterial) target;
        
		script.sprite = (Sprite) EditorGUILayout.ObjectField("Sprite", script.sprite, typeof(Sprite), false);
        
        script.shader = (Shader) EditorGUILayout.ObjectField("Shader", script.shader, typeof(Shader), false);

        script.mainColor = (Color) EditorGUILayout.ColorField("Main Color", script.mainColor);

        script.isNormalMapped = (bool) EditorGUILayout.Toggle("Has a Normal Map?", script.isNormalMapped);
		if (script.isNormalMapped) {
			script.normalMap = (Texture) EditorGUILayout.ObjectField("Normal Map", script.normalMap, typeof(Texture), false);
		}

		script.isShiny = (bool) EditorGUILayout.Toggle("Is Shiny?", script.isShiny);
		if (script.isShiny) {
			script.specularColor = (Color) EditorGUILayout.ColorField("Specular Color", script.specularColor);
			script.shininess = (float) EditorGUILayout.Slider("Shininess", script.shininess, 0.01f, 1f);
		}
	}
}
