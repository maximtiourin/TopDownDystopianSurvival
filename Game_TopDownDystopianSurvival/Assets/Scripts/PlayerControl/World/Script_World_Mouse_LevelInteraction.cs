using UnityEngine;
using System.Collections;

public class Script_World_Mouse_LevelInteraction : MonoBehaviour {
    public GameObject levelContainer;
    private Level level;
    private Camera camera;

	// Use this for initialization
	void Start () {
        level = (Level) levelContainer.GetComponent<Level>();
        camera = (Camera) gameObject.GetComponentInChildren<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        Vector3 pt = camera.ScreenToWorldPoint(Input.mousePosition);
        int x = (int) Mathf.Floor(pt.x);
        int y = (int) Mathf.Floor(pt.y);

        if (Input.GetMouseButtonUp(0)) {
            if (level.isValidTilePosition(x, y)) {
                level.createFloorTileAtPosition(x, y, "test01");
            }
        }
        else if (Input.GetMouseButtonUp(1)) {
            if (level.isValidTilePosition(x, y)) {
                level.createWallTileAtPosition(x, y, "test02");
            }
        }
    }
}
