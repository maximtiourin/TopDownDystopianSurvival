using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/*
 * Animation data container, consisting of animation frames, and flags describing the animation.
 * Author - Maxim Tiourin
 */
public class FizzikAnimation {
	protected List<FizzikFrame> frames = new List<FizzikFrame>();
	protected string name = "invalid";
	protected bool idle = false; // Whether or not the animation should animate
	protected bool loop = false; // Whether or not the animation should repeat after completing playthrough
	protected bool pingpong = false; // Whether or not the animation should play backwards once it reaches the end, requires looping

	/*
	 * Adds a frame to the end of the frame list
	 */
	public void AddFrame(FizzikFrame frame) {
		frames.Add(frame);
	}

	/*
	 * Returns the frame at the given index
	 */
	public FizzikFrame GetFrame(int index) {
		return frames[index];
	}

	/*
	 * How many frames are in the animation
	 */
	public int Size() {
		return frames.Count;
	}

	public string Name {
		get {
			return name;
		}
		set {
			name = value;
		}
	}

	public bool Idle {
		get {
			return idle;
		}
		set {
			idle = value;
		}
	}

	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
		}
	}

	public bool PingPong {
		get {
			return pingpong;
		}
		set {
			pingpong = value;
		}
	}
}
