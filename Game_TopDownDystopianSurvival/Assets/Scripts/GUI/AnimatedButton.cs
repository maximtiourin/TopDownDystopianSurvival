using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class AnimatedButton : Button {
    [Serializable]
    public class AnimatedButtonEvent : UnityEvent { }

    private Image img;
    private ButtonScript script;

    public bool shouldColorTintForAnimation = true;
    
    public AnimatedButtonEvent clickEvent = new AnimatedButtonEvent();

    protected override void Start() {
        base.Start();

        script = GetComponent<ButtonScript>();
        if (script != null) {
            onClick.AddListener(script.onClick);
        }
    }
    
    private void press() {
        clickEvent.Invoke();
    }

    public override void OnPointerClick(PointerEventData eventData) {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        press();

        base.OnPointerClick(eventData);
    }

    public override void OnSubmit(BaseEventData eventData) {
        press();

        base.OnSubmit(eventData);
    }

    protected override void OnValidate() {
        base.OnValidate();

        img = GetComponent<Image>();
    }

    protected override void DoStateTransition(SelectionState state, bool instant) {
        base.DoStateTransition(state, instant);

        /*
         * Animate ColorTint changes for animations if enabled
         */
        if (shouldColorTintForAnimation && transition == Transition.Animation) {
            if (state == SelectionState.Normal) {
                img.CrossFadeColor(colors.normalColor, colors.fadeDuration, true, true);
            }
            else if (state == SelectionState.Highlighted) {
                img.CrossFadeColor(colors.highlightedColor, colors.fadeDuration, true, true);
            }
            else if (state == SelectionState.Pressed) {
                img.CrossFadeColor(colors.pressedColor, colors.fadeDuration, true, true);
            }
            else if (state == SelectionState.Disabled) {
                img.CrossFadeColor(colors.disabledColor, colors.fadeDuration, true, true);
            }
        }
    }

}
