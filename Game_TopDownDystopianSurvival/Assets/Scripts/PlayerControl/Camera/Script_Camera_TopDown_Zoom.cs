using UnityEngine;
using System.Collections;

public class Script_Camera_TopDown_Zoom : MonoBehaviour {
	public float minimumDistance = 1f;
	public float maximumDistance = 1f;
	public float scrollAmount = .05f;
	//public float touchZoomMinimumDistance = 1f; //TOUCH TESTING
	public bool reverseZoom = false;

	//private Vector2 touchOneStartPos; //TOUCH TESTING
	//private Vector2 touchTwoStartPos; //TOUCH TESTING

	private Camera cam;

	// Use this for initialization
	void Start() {
		SetCamera(this.GetComponentInChildren<Camera>());

		//touchOneStartPos = Vector2.zero; //TOUCH TESTING
		//touchTwoStartPos = Vector2.zero;//TOUCH TESTING
	}
	
	// Update is called once per frame
	void Update() {
		//Mouse Wheel Scrolling
		if (cam != null) {
			float scroll = Input.GetAxis("Mouse ScrollWheel");
			
			if (scroll != 0f) {
				float dir = 0f;
				if (scroll > 0f) {
					if (reverseZoom) {
						dir = -1f;
					}
					else {
						dir = 1f;
					}
				}
				else {
					if (reverseZoom) {
						dir = 1f;
					}
					else {
						dir = -1f;
					}
				}
				
				float size = cam.orthographicSize;
				size *= 1f + (scrollAmount * dir);
				size = Mathf.Clamp(size, minimumDistance, maximumDistance);
				cam.orthographicSize = size;
			}
		}

		//2 Touch Screen Zooming
		/*if (Input.touchCount == 2) {
			//Store touches
			Touch touchOne = Input.GetTouch(0);
			Touch touchTwo = Input.GetTouch(1);

			//Check if touches have both began
			if (touchOne.phase == TouchPhase.Began && touchTwo.phase == TouchPhase.Began) {
				touchOneStartPos = touchOne.position;
				touchTwoStartPos = touchTwo.position;
			}

			if (touchOne.phase == TouchPhase.Stationary) {
				touchOneStartPos = touchOne.position;
			}

			if (touchTwo.phase == TouchPhase.Stationary) {
				touchTwoStartPos = touchTwo.position;
			}

			//Check if either touches has moved
			if (touchOne.phase == TouchPhase.Moved || touchTwo.phase == TouchPhase.Moved) {
				//Positions
				Vector2 touchOneCurPos = touchOne.position;
				Vector2 touchTwoCurPos = touchTwo.position;

				//Distances
				float startPosMagnitude = (touchOneStartPos - touchTwoStartPos).magnitude;
				float curPosMagnitude = (touchOneCurPos - touchTwoCurPos).magnitude;
				float touchDiff = curPosMagnitude - startPosMagnitude;

				//Check if we have have moved touches enough to effect zoom
				if (Mathf.Abs(touchDiff) >= touchZoomMinimumDistance) {
					//Decide zoom type
					float dir = 0f;
					if (touchDiff > 0f) {
						if (reverseZoom) {
							dir = -1f;
						}
						else {
							dir = 1f;
						}
					}
					else {
						if (reverseZoom) {
							dir = 1f;
						}
						else {
							dir = -1f;
						}
					}

					//Zoom
					float size = cam.orthographicSize;
					print("StartMag = " + startPosMagnitude + "\n" + "CurMag = " + curPosMagnitude + "\n" + "TouchDiff = " + touchDiff + "\n"
					      + "curSize = " + size + "\n" + "preClampSize = " + (size *= 1f + (dir * scrollAmount)) + "\n" + "clampSize = " + (Mathf.Clamp(size, minimumDistance, maximumDistance)));
					size *= 1f + (touchDiff * .0005f);
					size = Mathf.Clamp(size, minimumDistance, maximumDistance);
					cam.orthographicSize = size;
				}
			}
		}*/
	}

	public void SetCamera(Camera cam) {
		this.cam = cam;
	}
}
