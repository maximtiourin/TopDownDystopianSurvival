using UnityEngine;
using System.Collections;

public class Script_Rotation_LookAtMouse : MonoBehaviour {
	public Camera targetCamera;

	private const float tempRotate = -90f; //Rotational Offset to set sprites to default angle

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
		Vector3 point = new Vector3(ray.origin.x, ray.origin.y, transform.position.z);

		float angleRad = Mathf.Atan2(point.y - transform.position.y, point.x - transform.position.x);
		float angleDeg = angleRad * (180 / Mathf.PI);
		transform.rotation = Quaternion.Euler(0, 0, angleDeg + tempRotate);

		//transform.LookAt(point);
		//transform.RotateAround(transform.position, new Vector3(0, 0, 1), Vector3.Angle(transform.position, point));
	}
}
