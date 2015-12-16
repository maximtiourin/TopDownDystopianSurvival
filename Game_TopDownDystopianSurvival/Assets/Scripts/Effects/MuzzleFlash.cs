using UnityEngine;
using System.Collections;

/*
 * Creates an animated muzzle flash with an arbitrary amount of light sources
 * Author - Maxim Tiourin
 */
public class MuzzleFlash : MonoBehaviour {
	public Light[] lights;
	public float lightScale;
	public float lightScaleSpeed;
	public float duration;
	private float elapsed = 0f;

	private float[] initSpotAngles;

	// Use this for initialization
	void Start () {
		initSpotAngles = new float[lights.Length];
		for (int i = 0; i < lights.Length; i++) {
			initSpotAngles[i] = lights[i].spotAngle;
		}
	}
	
	// Update is called once per frame
	void Update () {
		if (elapsed > duration) {
			GameObject.Destroy(gameObject);
		}

		if (elapsed < (duration / 2)) {
			//Grow flash
			for (int i = 0; i < lights.Length; i++) {
				lights[i].spotAngle = Mathf.Lerp(lights[i].spotAngle, initSpotAngles[i] * lightScale, Time.deltaTime * lightScaleSpeed);
			}
		}
		else {
			//Shrink flash
			for (int i = 0; i < lights.Length; i++) {
				lights[i].spotAngle = Mathf.Lerp(lights[i].spotAngle, initSpotAngles[i], Time.deltaTime * lightScaleSpeed);
			}
		}

		elapsed += Time.deltaTime;
	}
}
