using UnityEngine;
using System.Collections;

/*
 * All encompassing class for arm 'control and animation' for characters
 * Author - Maxim Tiourin
 */
public class Script_Rotation_PlayerArmControl : MonoBehaviour {
	//Public Inspector-Enabled values
	public Camera targetCamera;
	public Transform leftArm;
	public Transform rightArm;
	public float minAimRangeAway = 1f; //Minimum Distance to consider aiming arms away from mouse
	public float maxAimRangeAway = 2.2f; //Maximum Distance to considering aiming arms away from mouse
	public float rotationSpeed = 8f; //Interpolated Rotation Speed

	// Translation/Rotation values
	private Quaternion? leftArmDesiredRotation;
	private Quaternion? leftArmDesiredLocalRotation;
	private Vector2? leftArmImpulseLocalPos;
	private float leftArmImpulseResetSpeed = 1f; //How fast the left arm should reset to its default position after an impulse
	private Quaternion? rightArmDesiredRotation;
	private Quaternion? rightArmDesiredLocalRotation;
	private Vector2? rightArmImpulseLocalPos;
	private float rightArmImpulseResetSpeed = 1f; //How fast the right arm should reset to its default position after an impulse

	//Helper variables
	private Vector2 mousePos;
	private float clampdistance; //Clamped distance of the aimAway
	private Vector2 leftArmDefaultLocalPos; //The default local position of the left arm
	private Vector2 rightArmDefaultLocalPos; //The default local position of the right arm

	//Constants
	private const float delta = .000001f; //Used for determining if a position is close enough
	private const float tempRotate = -90f; //Rotational Offset to set sprites to default angle

	// Use this for initialization
	void Start () {
		leftArmDefaultLocalPos = new Vector2(leftArm.localPosition.x, leftArm.localPosition.y);
		rightArmDefaultLocalPos = new Vector2(rightArm.localPosition.x, rightArm.localPosition.y);
	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
		mousePos = new Vector2(ray.origin.x, ray.origin.y);

		/* Determine what kind of rotation is needed */
		if (Input.GetButton("MouseRightClick")) {
			leftArmDesiredLocalRotation = SetRotationAwayFromMousePosition(leftArm, 180f, 90f);
			rightArmDesiredLocalRotation = SetRotationAwayFromMousePosition(rightArm, 0f, 90f);
		}
		else {
			//Set Arms Rotation To Mouse
			leftArmDesiredRotation = SetRotationTowardMousePosition(leftArm);
			rightArmDesiredRotation = SetRotationTowardMousePosition(rightArm);
		}

		/* Smoothly Update Rotation
         * While checking to see if collision with an obstacle occurs,
         * if it does, do nothing.
         */
		//Left Arm
		if (leftArmDesiredRotation.HasValue) {



			leftArm.rotation = Quaternion.Slerp(leftArm.rotation, leftArmDesiredRotation.Value, Time.deltaTime * rotationSpeed);
			leftArmDesiredRotation = null;
		}
		if (leftArmDesiredLocalRotation.HasValue) {
			leftArm.localRotation = Quaternion.Slerp(leftArm.localRotation, leftArmDesiredLocalRotation.Value, Time.deltaTime * rotationSpeed);
			leftArmDesiredLocalRotation = null;
		}
		//Right Arm
		if (rightArmDesiredRotation.HasValue) {
			rightArm.rotation = Quaternion.Slerp(rightArm.rotation, rightArmDesiredRotation.Value, Time.deltaTime * rotationSpeed);
			rightArmDesiredRotation = null;
		}
		if (rightArmDesiredLocalRotation.HasValue) {
			rightArm.localRotation = Quaternion.Slerp(rightArm.localRotation, rightArmDesiredLocalRotation.Value, Time.deltaTime * rotationSpeed);
			rightArmDesiredLocalRotation = null;
		}
	}

	void FixedUpdate() {
		/* Smoothly Update Local Position, taking into account impulses */
		if (leftArmImpulseLocalPos.HasValue) {
			Vector2 forward = Vector2.up;
			Vector2 side = Vector2.right;
			Vector2 offset = new Vector2(side.x * leftArmImpulseLocalPos.Value.x, forward.y * leftArmImpulseLocalPos.Value.y);

			leftArm.localPosition = leftArmDefaultLocalPos + offset;

			/*Debug.Log("TransformForward: " + leftArm.transform.up + ", Forward: " + forward + ", Side: " + side + 
			          ", Offset: (" + offset.x + ", " + offset.y + "), DefaultLocalPos: " + leftArmDefaultLocalPos + 
			          ", NewLocalPos: (" + leftArm.localPosition.x + ", " + leftArm.localPosition.y + ")");*/
			
			float x = leftArmImpulseLocalPos.Value.x;
			float y = leftArmImpulseLocalPos.Value.y;
			float lerpx = Mathf.Lerp(x, 0f, Time.fixedDeltaTime * leftArmImpulseResetSpeed);
			float lerpy = Mathf.Lerp(y, 0f, Time.fixedDeltaTime * leftArmImpulseResetSpeed);
			leftArmImpulseLocalPos = new Vector2(lerpx, lerpy);

			Vector2 vec = leftArmImpulseLocalPos.Value;
			if ((Mathf.Abs(vec.x) <= delta) && (Mathf.Abs(vec.y) <= delta)) {
				leftArmImpulseLocalPos = null;
				leftArmImpulseResetSpeed = 1f;
			}
		}
		if (rightArmImpulseLocalPos.HasValue) {
			Vector2 forward = Vector2.up;
			Vector2 side = Vector2.right;
			Vector2 offset = new Vector2(side.x * rightArmImpulseLocalPos.Value.x, forward.y * rightArmImpulseLocalPos.Value.y);
			
			rightArm.localPosition = rightArmDefaultLocalPos + offset;
			
			float x = rightArmImpulseLocalPos.Value.x;
			float y = rightArmImpulseLocalPos.Value.y;
			float lerpx = Mathf.Lerp(x, 0f, Time.fixedDeltaTime * rightArmImpulseResetSpeed);
			float lerpy = Mathf.Lerp(y, 0f, Time.fixedDeltaTime * rightArmImpulseResetSpeed);
			rightArmImpulseLocalPos = new Vector2(lerpx, lerpy);

			Vector2 vec = rightArmImpulseLocalPos.Value;
			if ((Mathf.Abs(vec.x) <= delta) && (Mathf.Abs(vec.y) <= delta)) {
				rightArmImpulseLocalPos = null;
				rightArmImpulseResetSpeed = 1f;
			}
		}
	}

	/*
	 * Adds an impulse value towards this transform's position, treats the supplied vector
	 * as if the transform was in it's default local position.
	 */
	public void AddImpulseTranslation(Vector2 impulse, float resetSpeed, Transform t) {
		if (leftArm.Equals(t)) {
			leftArmImpulseLocalPos = impulse;
			leftArmImpulseResetSpeed = resetSpeed;
		}
		else if (rightArm.Equals(t)) {
			rightArmImpulseLocalPos = impulse;
			rightArmImpulseResetSpeed = resetSpeed;
		}
	}

    private bool CheckArmForObstacleCollision(Transform arm) {
        Collider2D col = arm.GetComponent<Collider2D>();

        //int layermask = 


        return false;
    }

	/*
	 * Returns a quaternion rotation from the pivot of the transform towards the mouse position
	 */
	private Quaternion SetRotationTowardMousePosition(Transform t) {
		Vector2 pos = new Vector2(t.position.x, t.position.y); //t pos

		//float distance = Vector2.Distance(pos, mousePos); Not currently used.

		float angleRad = Mathf.Atan2(mousePos.y - pos.y, mousePos.x - pos.x);
		float angleDeg = angleRad * (180 / Mathf.PI);

		Quaternion newrot = Quaternion.Euler(0, 0, angleDeg + tempRotate);

		return newrot;
	}

	/*
	 * Returns a quaternion local rotation that is clamped between minAngle and maxAngle based on the distance
	 * the mouse is between minAimRangeAway and maxAimRangeAway. [Visualized as Gizmo]
	 */
	private Quaternion SetRotationAwayFromMousePosition(Transform t, float minAngle, float maxAngle) {
		Vector2 pos = new Vector2(t.position.x, t.position.y);

		float diff = maxAimRangeAway - minAimRangeAway; //Difference between max and min range;
		float distance = Vector2.Distance(pos, mousePos); //Mouse's distance from t
		clampdistance = Mathf.Clamp(distance, minAimRangeAway, maxAimRangeAway); //Clamped distance

		float percent = ((clampdistance - minAimRangeAway) / diff); //Percent distance we are towards max distance from min distance

		float angleDeg = minAngle + ((maxAngle - minAngle) * percent);

		Quaternion newrot = Quaternion.Euler(0, 0, angleDeg + tempRotate);

		return newrot;
	}

	/*
	 * Draw Gizmos
	 * - Mouse Position as percentage of range between minAimRangeAway, maxAimRangeAway [Green Line: range, Red Line Perp To Green Line: mouse as percentage]
	 */
	void OnDrawGizmosSelected() {
		/* Helper variables */
		float lw = 1f; //Mouse Line Width
		float hlw = lw / 2f; //Half Mouse Line Width

		/* Left Arm */
		// minAimRangeAway to maxAimRangeAway
		Gizmos.color = Color.green;
		Gizmos.DrawLine(leftArm.position + (leftArm.up * minAimRangeAway), leftArm.position + (leftArm.up * maxAimRangeAway));
		// clampdistance
		Gizmos.color = Color.red;
		Gizmos.DrawLine(leftArm.position + ((-leftArm.right) * hlw) + (leftArm.up * (clampdistance)), leftArm.position + ((leftArm.right) * hlw) + (leftArm.up * (clampdistance)));

		/* Right Arm */		
		// minAimRangeAway to maxAimRangeAway
		Gizmos.color = Color.green;
		Gizmos.DrawLine(rightArm.position + (rightArm.up * minAimRangeAway), rightArm.position + (rightArm.up * maxAimRangeAway));		
		// clampdistance
		Gizmos.color = Color.red;
		Gizmos.DrawLine(rightArm.position + ((-rightArm.right) * hlw) + (rightArm.up * (clampdistance)), rightArm.position + ((rightArm.right) * hlw) + (rightArm.up * (clampdistance)));
	}
}
