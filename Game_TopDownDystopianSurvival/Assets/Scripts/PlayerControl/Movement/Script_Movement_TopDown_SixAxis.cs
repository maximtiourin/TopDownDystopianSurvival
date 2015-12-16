using UnityEngine;
using System.Collections;

public class Script_Movement_TopDown_SixAxis : MonoBehaviour {
	public float scalarMovementSpeed = 1f;

	private float scalarDiagonal = .7071f;

	// Use this for initialization
	void Start() {
	
	}
	
	// Update is called once per frame
	void Update() {
	
	}

	void FixedUpdate() {
		Rigidbody2D body = GetComponent<Rigidbody2D>();

		bool vertical = Input.GetAxis("Vertical") != 0f;
		bool horizontal = Input.GetAxis("Horizontal") != 0f;

		float ax = Input.GetAxis("Horizontal");
		float ay = Input.GetAxis("Vertical");

		if (horizontal && vertical) {
			ax *= scalarDiagonal;
			ay *= scalarDiagonal;
		}

		ax *= scalarMovementSpeed;
		ay *= scalarMovementSpeed;

		body.velocity = new Vector2(ax, ay);
	}
}
