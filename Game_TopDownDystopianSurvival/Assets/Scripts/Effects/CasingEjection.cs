using UnityEngine;
using System.Collections;

/*
 * Creates an animated bullet casing ejection that collides with the background.
 * Author - Maxim Tiourin
 */
public class CasingEjection : MonoBehaviour {
	public Vector2 minEjectionForce;
	public Vector2 maxEjectionForce;
	public float ejectionAngleRange;
	public float maxEjectionTorque = 300f; //What the maximum negative/positive torque can be upon ejection
	public float maxCasingAge = 75; // How many seconds before the casing despawns
	public float casingFadeoutCutoff = 10f; // How many seconds before casing dies of old age to start fading
	public float casingFadeoutSpeed = 1f; // How fast to fadeout the casing

	private Material material;
	private float ejectionForce;
	private float ejectionAngle;
	private float casingAge; // Current Age

	private bool matinit = false; //Material has been initialized
	private bool init = false; //Casing has been initialized

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (!matinit) {
			material = GetComponent<SpriteRenderer>().material;

			matinit = true;
		}

		if (casingAge > maxCasingAge) {
			GameObject.Destroy(gameObject);
		}

		if (casingAge > (maxCasingAge - casingFadeoutCutoff)) {
			Color oldcolor = material.GetColor("_Color");
			material.SetColor("_Color", new Color(oldcolor.r, oldcolor.g, oldcolor.b, Mathf.Lerp(oldcolor.a, 0f, Time.deltaTime * casingFadeoutSpeed)));
		}

		casingAge += Time.deltaTime;
	}

	void FixedUpdate () {
		if (!init) {
			ejectionForce = Random.Range(minEjectionForce.magnitude, maxEjectionForce.magnitude);
			ejectionAngle = Random.Range(0f, ejectionAngleRange);
			
			if (ejectionAngle > (ejectionAngleRange / 2)) ejectionAngle = (ejectionAngleRange - ejectionAngle);
			else ejectionAngle = -ejectionAngle;
			
			float ejectAngleRad = Mathf.Atan2(maxEjectionForce.y, maxEjectionForce.x);

			Quaternion rot = this.transform.rotation;
			float rotz = rot.eulerAngles.z;
			
			Rigidbody2D body = GetComponent<Rigidbody2D>();
			body.AddForce(new Vector2(ejectionForce * Mathf.Cos(ejectAngleRad + (Mathf.Deg2Rad * ejectionAngle) + (Mathf.Deg2Rad * rotz)), ejectionForce * Mathf.Sin(ejectAngleRad + (Mathf.Deg2Rad * ejectionAngle) + (Mathf.Deg2Rad * rotz))));
			body.AddTorque(Random.Range(-maxEjectionTorque, maxEjectionTorque), ForceMode2D.Impulse);

			init = true;
		}
	}

	void OnDrawGizmosSelected() {
		Vector3 pos = this.transform.position;
		float ejectAngleRad = Mathf.Atan2(maxEjectionForce.y, maxEjectionForce.x);
		float hej = ejectionAngleRange / 2;
		Vector3 leftAngle = new Vector3(maxEjectionForce.magnitude * Mathf.Cos(ejectAngleRad - (Mathf.Deg2Rad * (hej))), maxEjectionForce.magnitude * Mathf.Sin(ejectAngleRad - (Mathf.Deg2Rad * (hej))), pos.z);
		Vector3 rightAngle = new Vector3(maxEjectionForce.magnitude * Mathf.Cos(ejectAngleRad + (Mathf.Deg2Rad * (hej))), maxEjectionForce.magnitude * Mathf.Sin(ejectAngleRad + (Mathf.Deg2Rad * (hej))), pos.z);

		Gizmos.color = Color.green;
		Gizmos.DrawLine(pos, new Vector3(pos.x + maxEjectionForce.x, pos.y + maxEjectionForce.y, pos.z));
		Gizmos.color = Color.cyan;
		Gizmos.DrawLine(pos, new Vector3(pos.x + minEjectionForce.x, pos.y + minEjectionForce.y, pos.z));
		Gizmos.color = Color.red;
		Gizmos.DrawLine(pos, pos + leftAngle);
		Gizmos.DrawLine(pos, pos + rightAngle);
	}
}
