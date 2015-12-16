using UnityEngine;
using System.Collections;

public class Anim_TestPistol_Fire : MonoBehaviour {
	public Sprite spriteIdle;
	public Sprite spriteFire;
	
	void Start() {
		FizzikAnimation anim = new FizzikAnimation();
		
		anim.Name = "fire";
		
		FizzikFrame frame;
		
		//Frame 1
		frame = new FizzikFrame(spriteIdle, .025f);
		anim.AddFrame(frame);
		frame = new FizzikFrame(spriteFire, .075f);
		anim.AddFrame(frame);
		frame = new FizzikFrame(spriteIdle, .025f);
		anim.AddFrame(frame);
		//

		FizzikAnimationController controller = this.GetComponentInParent<FizzikAnimationController>();
		
		controller.AddAnimation(anim);
	}
	
	void Update() {
		
	}
}
