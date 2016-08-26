using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEditor;

[CustomEditor(typeof(AnimatedButton))]
public class EditorScript_AnimatedButton : Editor {
    AnimatedButton btn;

    public override void OnInspectorGUI() {
        btn = (AnimatedButton) target;

        btn.interactable = EditorGUILayout.Toggle("Interactable", btn.interactable);
        btn.transition = (Selectable.Transition) EditorGUILayout.Popup("Transition", (int) btn.transition, 
            new string[] {
                Selectable.Transition.None.ToString(),
                Selectable.Transition.ColorTint.ToString(),
                Selectable.Transition.SpriteSwap.ToString(),
                Selectable.Transition.Animation.ToString()
            });

        //Display appropriate Transition Information
        EditorGUI.indentLevel++;
        if (btn.transition == Selectable.Transition.Animation) {
            btn.shouldColorTintForAnimation = EditorGUILayout.Toggle("Animate Color", btn.shouldColorTintForAnimation);
        }
        if (btn.transition == Selectable.Transition.ColorTint 
            || btn.transition == Selectable.Transition.SpriteSwap 
            || (btn.transition == Selectable.Transition.Animation && btn.shouldColorTintForAnimation)) 
        {
            btn.targetGraphic = (Image) EditorGUILayout.ObjectField("Target Graphic", btn.targetGraphic, typeof(Image), false);
        }
        if (btn.transition == Selectable.Transition.ColorTint
            || (btn.transition == Selectable.Transition.Animation && btn.shouldColorTintForAnimation)) {
            ColorBlock colorBlock = new ColorBlock();
            colorBlock.normalColor = EditorGUILayout.ColorField("Normal Color", btn.colors.normalColor);
            colorBlock.highlightedColor = EditorGUILayout.ColorField("Highlighted Color", btn.colors.highlightedColor);
            colorBlock.pressedColor = EditorGUILayout.ColorField("Pressed Color", btn.colors.pressedColor);
            colorBlock.disabledColor = EditorGUILayout.ColorField("Disabled Color", btn.colors.disabledColor);
            colorBlock.colorMultiplier = EditorGUILayout.Slider("Color Multiplier", btn.colors.colorMultiplier, 1f, 5f);
            colorBlock.fadeDuration = EditorGUILayout.FloatField("Fade Duration", btn.colors.fadeDuration);
            btn.colors = colorBlock;
        }
        if (btn.transition == Selectable.Transition.SpriteSwap) {
            SpriteState spriteState = new SpriteState();
            spriteState.highlightedSprite = (Sprite) EditorGUILayout.ObjectField("Highlighted Sprite", btn.spriteState.highlightedSprite, typeof(Sprite), false, GUILayout.Height(16));
            spriteState.pressedSprite = (Sprite) EditorGUILayout.ObjectField("Pressed Sprite", btn.spriteState.pressedSprite, typeof(Sprite), false, GUILayout.Height(16));
            spriteState.disabledSprite = (Sprite) EditorGUILayout.ObjectField("Disabled Sprite", btn.spriteState.disabledSprite, typeof(Sprite), false, GUILayout.Height(16));
            btn.spriteState = spriteState;
        }
        if (btn.transition == Selectable.Transition.Animation) {
            btn.animationTriggers.normalTrigger = EditorGUILayout.TextField("Normal Trigger", btn.animationTriggers.normalTrigger);
            btn.animationTriggers.highlightedTrigger = EditorGUILayout.TextField("Highlighted Trigger", btn.animationTriggers.highlightedTrigger);
            btn.animationTriggers.pressedTrigger = EditorGUILayout.TextField("Pressed Trigger", btn.animationTriggers.pressedTrigger);
            btn.animationTriggers.disabledTrigger = EditorGUILayout.TextField("Disabled Trigger", btn.animationTriggers.disabledTrigger);
        }
        EditorGUI.indentLevel--;

        GUILayout.Space(8);

        //Navigation Workaround
        Navigation.Mode mode = (Navigation.Mode) EditorGUILayout.Popup("Navigation", (int) btn.navigation.mode,
            new string[] {
                Navigation.Mode.None.ToString(),
                Navigation.Mode.Horizontal.ToString(),
                Navigation.Mode.Vertical.ToString(),
                Navigation.Mode.Automatic.ToString(),
                Navigation.Mode.Explicit.ToString()
            });
        Navigation nav = new Navigation();
        nav.mode = mode;
        btn.navigation = nav;
    }
}