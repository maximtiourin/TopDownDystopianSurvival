using UnityEngine;
using System.Collections;

namespace Weapons {
	/*
	 * Abstract Base Class for a generic weapon.
	 */
	public abstract class Weapon : MonoBehaviour {
		//public Vector2 posOffset; // How much to offset the position from origin in local space

		//public bool hasCollider; // Whether or not this weapon has a box collider
		//public Vector2 colliderOffset; // How much to offset the collider position from origin
		//public Vector2 colliderSize; // How much to scale the size of the collider

		protected void Start () {

		}

		abstract protected void Update ();
	}
}
