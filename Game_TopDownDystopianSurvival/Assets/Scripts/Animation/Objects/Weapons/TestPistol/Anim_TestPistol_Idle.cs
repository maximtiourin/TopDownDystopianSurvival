using UnityEngine;
using System.Collections;

public class Anim_TestPistol_Idle : MonoBehaviour {
	public Sprite spriteIdle;

	void Start() {
		FizzikAnimation anim = new FizzikAnimation();

		anim.Name = "idle";
		anim.Idle = true;

		FizzikFrame frame;

		//Frame 1
		frame = new FizzikFrame(spriteIdle, 1f);
		anim.AddFrame(frame);
		//

		FizzikAnimationController controller = this.GetComponentInParent<FizzikAnimationController>();

		controller.AddAnimation(anim);
	}

	void Update() {

	}
}
