using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Script_World_Mouse_LevelInteraction : MonoBehaviour {
    public GameObject levelContainer;
    public GameObject eventSystemContainer;
    private Level level;
    private EventSystem eventSystem;
    private Camera camera;

	// Use this for initialization
	void Start () {
        level = (Level) levelContainer.GetComponent<Level>();
        eventSystem = eventSystemContainer.GetComponent<EventSystem>();
        camera = (Camera) gameObject.GetComponentInChildren<Camera>();
	}
	
	// Update is called once per frame
	void Update () {
        if (!eventSystem.IsPointerOverGameObject()) {
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
}
