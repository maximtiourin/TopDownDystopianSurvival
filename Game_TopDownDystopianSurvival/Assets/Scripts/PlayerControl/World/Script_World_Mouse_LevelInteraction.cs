using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class Script_World_Mouse_LevelInteraction : MonoBehaviour {
    public GameObject levelContainer;
    public GameObject eventSystemContainer;
    private Level level;
    private EventSystem eventSystem;
    private Camera camera;

    private bool obstructed;
    private float mousex;
    private float mousey;
    private int tilex;
    private int tiley;

	// Use this for initialization
	void Start () {
        level = (Level) levelContainer.GetComponent<Level>();
        eventSystem = eventSystemContainer.GetComponent<EventSystem>();
        camera = (Camera) gameObject.GetComponentInChildren<Camera>();

        obstructed = false;
        mousex = 0f;
        mousey = 0f;
        tilex = 0;
        tiley = 0;
	}
	
	// Update is called once per frame
	void Update () {
        if (!eventSystem.IsPointerOverGameObject()) {
            obstructed = false;

            Vector3 pt = camera.ScreenToWorldPoint(Input.mousePosition);
            mousex = pt.x;
            mousey = pt.y;
            int x = (int) Mathf.Floor(mousex);
            int y = (int) Mathf.Floor(mousey);

            if (level.isValidTilePosition(x, y)) {
                tilex = x;
                tiley = y;

                if (Input.GetMouseButtonUp(0)) {
                    level.createFloorTileAtPosition(x, y, "test01");
                }
                else if (Input.GetMouseButtonUp(1)) {
                    level.createWallTileAtPosition(x, y, "test02");
                }
                else if (Input.GetMouseButtonUp(2)) {
                    level.createTestDoorAtPosition(x, y);
                }
            }
        }
        else {
            obstructed = true;
        }
    }

    public bool isObstructed() {
        return obstructed;
    }

    public float getMouseX() {
        return mousex;
    }

    public float getMouseY() {
        return mousey;
    }

    public int getTileX() {
        return tilex;
    }

    public int getTileY() {
        return tiley;
    }
}
