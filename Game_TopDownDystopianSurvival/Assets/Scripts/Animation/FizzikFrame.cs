using UnityEngine;
using System.Collections;

/*
 * Container object for an animation frame, containing a sprite and its duration
 * Author - Maxim Tiourin
 */
public class FizzikFrame {
	private Sprite sprite;
	private float duration;

	public FizzikFrame(Sprite sprite, float duration) {
		this.sprite = sprite;
		this.duration = duration;
	}

	public Sprite SpriteImage {
		get {
			return sprite;
		}
	}

	public float Duration {
		get {
			return duration;
		}
	}
}
