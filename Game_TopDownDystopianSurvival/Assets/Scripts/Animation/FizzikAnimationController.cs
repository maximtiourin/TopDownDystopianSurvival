using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Wonky Custom Animation controller to handle sprite-based image-replacement animation better than Unity allows, while still
 * doing it in a rather stupid way to work around unity's paradigm.
 * Author - Maxim Tiourin
 */
public class FizzikAnimationController : MonoBehaviour {
	public string defaultAnimation = "idle"; //If an animation doesn't loop, it will play the default animation after completion.
	public float animationSpeed = 1f;

	protected List<FizzikAnimation> animations = new List<FizzikAnimation>();
	protected FizzikAnimation currentAnim;
	protected int currentFrame = 0;
	protected float currentPlaytime = 0f;
	protected bool pingpong = false;
	protected SpriteRenderer renderer;
	protected bool init = false;
	
	// Use this for initialization
	void Start () {
		renderer = GetComponent<SpriteRenderer>();
	}

	protected void Init() {		
		PlayAnimation(defaultAnimation);
	}
	
	// Update is called once per frame
	void Update () {
		if (!init) {
			Init();

			init = true;
		}

		if (currentAnim != null) {
			if (!currentAnim.Idle) {
				FizzikFrame frame = currentAnim.GetFrame(currentFrame);

				if (currentPlaytime >= frame.Duration) {
					//Finished Frame
					if (IsLastFrame()) {
						//Reached Last Frame
						if (currentAnim.Loop) {
							if (currentAnim.PingPong) {
								//Ping Pong Loop
								pingpong = !pingpong;
								PlayNextFrame();
							}
							else {
								//Regular Loop
								PlayAnimation(currentAnim.Name);
							}
						}
						else {
							//Switch back to default
							PlayAnimation(defaultAnimation);
						}
					}
					else {
						//Go to next Frame
						PlayNextFrame();
					}
				}

				currentPlaytime += Time.deltaTime * animationSpeed;
			}
		}
	}

	/*
	 * Plays the animation with the given identifier name, can be used to replay an animation
	 * easily, without havine the overhead of searching for the animation given an identifier.
	 */
	public void PlayAnimation(string animation) {
		FizzikAnimation anim;

		if (currentAnim != null && animation.Equals(currentAnim.Name)) {
			anim = currentAnim;
		}
		else {
			anim = FindAnimation(animation);
		}

		if (anim != null) {
			currentAnim = anim;
			currentFrame = 0;
			currentPlaytime = 0f;
			pingpong = false;

			renderer.sprite = currentAnim.GetFrame(currentFrame).SpriteImage;
		}
	}

	/*
	 * Set the animation speed scale
	 */
	public void SetAnimationSpeed(float speed) {
		animationSpeed = speed;
	}

	protected void PlayNextFrame() {
		currentPlaytime = 0;

		if (currentAnim.Loop && currentAnim.PingPong && pingpong) {
			currentFrame -= 1;
		}
		else {
			currentFrame += 1;
		}

		renderer.sprite = currentAnim.GetFrame(currentFrame).SpriteImage;
	}

	public void AddAnimation(FizzikAnimation anim) {
		animations.Add(anim);
	}

	/*
	 * Returns whether or not this is the last frame for the current animation,
	 * taking into account ping pong states
	 */
	protected bool IsLastFrame() {
		if (currentAnim.Loop && currentAnim.PingPong && pingpong) {
			if (currentFrame <= 0) return true;
		}
		else {
			if (currentFrame >= currentAnim.Size() - 1) return true;
		}

		return false;
	}

	protected FizzikAnimation FindAnimation(string animation) {
		foreach (FizzikAnimation anim in animations) {
			if (anim.Name.Equals(animation)) return anim;
		}

		return null;
	}
}
