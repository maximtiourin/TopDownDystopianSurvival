using UnityEngine;
using System.Collections;

namespace Weapons {
	/*
	 * Abstract Base class for a generic ranged weapon, which inherits from Weapon
	 * Author - Maxim Tiourin
	 */
	public abstract class RangedWeapon : Weapon {
		public enum FireModes {
			None = 0,
			Semi = 1,
			Auto = 2,
			SemiAndAuto = 3
		}

		public Vector2 fireOffset; // How much to offset the position from where the bullet will be fired from.
		public Vector2 casingOffset; // How much to offset the position from where the bullet casing will be ejected from.
		public GameObject gunfirePrefab; // Prefab that appears near the fireOffset, simulating residue, flash, sound, etc.
		public GameObject casingPrefab; // Prefab that appears near the casingOffset, simulating a bullet casing being ejected.
		public FireModes fireModes; // Modes of fire that this ranged weapon supports.
		public bool dualWieldable; // Whether or not two of this weapon can be held at once
		public float projectileDamage; // How much damage a single projectile from this weapon does
		public float maxProjectileRange; // How far the projectile can travel before being rendered inert
		public float minProjectileFalloffRange; // How far the projectile can travel before it starts losing damage, up to a total of 100% damage loss at maxProjectileRange
		public Vector2 kickImpulse = Vector2.zero; // What kind of impulse to apply on weapon kick.
		public float kickResetSpeed = 1f; // How fast the arm holding the weapon should reset after a kickback occurs

		protected int casingRollover = 10000; // Rollover the casing count after this many casing instance increments for this weapon
		protected int casingCounter; // Keeps track of how many bullet casings we have, to help increase their sorting order to prevent blending
		protected bool lateInit = false;

		protected void Start () {
			base.Start();

			casingCounter = 0;
		}

		/*
		 * Late initialization, useful for instantiated objects
		 */
		protected void LateInit() {
			//Set our sorting order to parent arm's sorting order + 1
			this.gameObject.GetComponent<SpriteRenderer>().sortingOrder = this.transform.parent.gameObject.GetComponent<SpriteRenderer>().sortingOrder + 1;

			lateInit = true;
		}

		protected abstract void FireRound();

		protected void MuzzleFlash() {
			GameObject muzzle = GameObject.Instantiate(gunfirePrefab);
			muzzle.transform.parent = this.transform;
			muzzle.transform.rotation = muzzle.transform.parent.rotation;
			muzzle.transform.localPosition = new Vector3(fireOffset.x, fireOffset.y, this.transform.position.z);
		}

		protected void EjectCasing() {
			Vector2 transform = new Vector2(this.transform.position.x, this.transform.position.y);
			Vector2 faceDir = new Vector2(this.transform.up.x, this.transform.up.y);
			Vector2 faceDirPerp = new Vector2(this.transform.right.x, this.transform.right.y);

			GameObject casing = GameObject.Instantiate(casingPrefab);
			SpriteRenderer casingRender = casing.GetComponent<SpriteRenderer>();
			casingRender.sortingOrder = casingRender.sortingOrder + casingCounter;
			casingCounter += 1;
			casingCounter = casingCounter % casingRollover;
			Vector3 casepos = transform + (faceDirPerp * casingOffset.x) + (faceDir * casingOffset.y);
			casing.transform.position = new Vector3(casepos.x, casepos.y, this.transform.position.z);
			casing.transform.rotation = this.transform.rotation;
		}

		void OnDrawGizmosSelected() {
			Vector3 transform = gameObject.transform.position;
			Vector3 faceDir = gameObject.transform.up;
			Vector3 faceDirPerp = gameObject.transform.right;
			
			//Casing Offset
			Gizmos.color = Color.red;
			Gizmos.DrawSphere(transform + (faceDirPerp * casingOffset.x) + (faceDir * casingOffset.y), 0.01f);
			
			//Fire Ray
			Vector3 rayFrom = transform + (faceDirPerp * fireOffset.x) + (faceDir * fireOffset.y);
			Vector3 rayTo = transform + (faceDirPerp * fireOffset.x) + (faceDir * (fireOffset.y + maxProjectileRange));
			Gizmos.color = Color.green;
			Gizmos.DrawLine(rayFrom, rayTo);
		}
	}
}
