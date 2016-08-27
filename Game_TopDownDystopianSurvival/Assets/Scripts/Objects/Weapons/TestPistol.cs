using UnityEngine;
using System.Collections;

namespace Weapons {
	/*
	 * TestPistol Ranged Weapon
	 * Author - Maxim Tiourin
	 */
	public class TestPistol : RangedWeapon {
		private float hitForce = 80f; //Temp force for when the bullet hits something
		private FizzikAnimationController animControl;
		private Transform arm;
		private Script_Rotation_PlayerArmControl armController;

		// Use this for initialization
		protected void Start () {
			base.Start();

			animControl = GetComponent<FizzikAnimationController>();
			arm = transform.parent;
			armController = transform.parent.parent.GetComponent<Script_Rotation_PlayerArmControl>();
		}

		// Update is called once per frame
		override protected void Update () {
			if (!lateInit) {
				LateInit();
			}

			if (Input.GetMouseButtonDown(0)) {
				FireRound();
			}
		}

		override protected void FireRound() {
			Vector2 transform = new Vector2(this.transform.position.x, this.transform.position.y);
			Vector2 faceDir = new Vector2(this.transform.up.x, this.transform.up.y);
			Vector2 faceDirPerp = new Vector2(this.transform.right.x, this.transform.right.y);
			Vector2 rayPos = transform + (faceDirPerp * fireOffset.x) + (faceDir * fireOffset.y);

			//Muzzle Flash
			MuzzleFlash();

			//Casing Ejection
			EjectCasing();

			//Animate
			animControl.PlayAnimation("fire");

            //Ray cast
            int layermask = LayerMask.GetMask("Obstacle");
			RaycastHit2D hit = Physics2D.Raycast(rayPos, faceDir, maxProjectileRange, layermask); 
			if (hit.collider != null) {
				hit.collider.attachedRigidbody.AddForceAtPosition((hit.point - transform).normalized * hitForce, hit.point);
			}

			//Apply Kick (after initial fire)
			armController.AddImpulseTranslation(kickImpulse, kickResetSpeed, arm);
		}
	}
}
