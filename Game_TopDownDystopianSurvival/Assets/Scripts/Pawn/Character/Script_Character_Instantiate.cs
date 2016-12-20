using UnityEngine;
using System.Collections;

/*
 * Instantiates a basic character, that can later have player control, or ai
 * control added on to it.
 * 
 * NOT CURRENTLY IMPLEMENTED, OR PLANNED FOR USE
 * 
 * Author - Maxim Tiourin
 */
public class Script_Character_Instantiate : MonoBehaviour {
	private Transform root;
	private Transform body;

	// Use this for initialization
	/*void Start () {
		//Create character root
		root = this.transform;
		root.gameObject.AddComponent(Rigidbody2D);
		Rigidbody2D rigidbody = root.GetComponent<Rigidbody2D>();
		rigidbody.mass = 1f;
		rigidbody.drag = 0f;
		rigidbody.angularDrag = .05f;
		rigidbody.gravityScale = 1f;
		rigidbody.fixedAngle = true;
		rigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;

		//Create body root
		body = (new GameObject("body_root")).transform;
	}*/
	
	// Update is called once per frame
	void Update () {
	
	}
}
