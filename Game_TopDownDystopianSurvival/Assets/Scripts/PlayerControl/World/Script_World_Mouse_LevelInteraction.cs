using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;

public class Script_World_Mouse_LevelInteraction : MonoBehaviour {
    public GameObject levelContainer;
    public GameObject eventSystemContainer;
    public Pawn selectedPawn; //DEBUG
    private Level level;
    private EventSystem eventSystem;
    private Camera camera;

    private RaycastHit2D[] selectionHits;
    private int selectionHitsIndex;

    private bool obstructed;
    private float mousex;
    private float mousey;
    private int tilex;
    private int tiley;

    //TODO - WORK IN PROGRESS - Drag selection of all pawns
    private int DRAG_RECOGNIZE_TIME = 250; //Time in milliseconds
    private Vector2 leftMouseDownPos;
    private bool leftMouseDown;
    private bool leftMouseDragging;

	// Use this for initialization
	void Start () {
        level = (Level) levelContainer.GetComponent<Level>();
        eventSystem = eventSystemContainer.GetComponent<EventSystem>();
        camera = (Camera) gameObject.GetComponentInChildren<Camera>();

        selectionHits = null;
        selectionHitsIndex = 0;

        obstructed = false;
        mousex = 0f;
        mousey = 0f;
        tilex = 0;
        tiley = 0;

        leftMouseDown = false;
        leftMouseDragging = false;
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
                
                //TODO - WORK IN PROGRESS -Drag selection of all pawns
                if (Input.GetMouseButton(0)) {
                    leftMouseDown = true;
                }
                else {
                    leftMouseDown = false;
                    leftMouseDragging = false;
                }

                //TODO TEMPORARY DEBUG INTERACTION
                if (Input.GetMouseButtonUp(0)) {
                    //Select a pawn at the mouse, cycling through layered pawns if necessary (if the layering order of those pawns has not changed between clicks)
                    int layermask = LayerMask.GetMask("Pawn");
                    RaycastHit2D[] hits = Physics2D.RaycastAll(new Vector2(mousex, mousey), Vector2.zero, 0f, layermask);

                    if (hits.Length > 0) {
                        Pawn selection = hits[0].transform.gameObject.GetComponent<PawnComponent>().pawn;
                        if (selectionHits != null) {
                            if (areSelectionHitsEqualAndValid(selectionHits, hits)) {
                                RaycastHit2D? next = nextSelectionHit();
                                if (next.HasValue) {
                                    selection = next.Value.transform.gameObject.GetComponent<PawnComponent>().pawn;
                                }
                            }
                            else {
                                resetSelectionHits(hits);
                            }
                        }
                        else {
                            resetSelectionHits(hits);
                        }

                        selectedPawn = selection;
                    }
                    else {
                        //Unselect
                        selectedPawn = null;
                        resetSelectionHits(null);
                    }
                }
                else if (Input.GetMouseButtonUp(1)) {
                    level.createWallTileAtPosition(x, y, "test02"); //wall
                }
                else if (Input.GetMouseButtonUp(2)) {
                    level.createTestDoorAtPosition(x, y); //door
                }
                else if (Input.GetMouseButtonUp(3)) {
                    TestCharacter.CreateTestCharacterAtWorldPosition(level, level.getCenterWorldPositionAtLevelPosition(x, y)); //test pawn
                }
                else if (Input.GetMouseButtonUp(4)) {
                    level.createFloorTileAtPosition(x, y, "test01"); //floor
                }

                //TODO TEMPORARY GAMESTATE SPEED INTERACTION
                if (Input.GetKeyUp(KeyCode.Alpha1)) {
                    GameTime.setSpeedState(GameTime.SPEED_STATE_NORMAL);
                }
                else if (Input.GetKeyUp(KeyCode.Alpha2)) {
                    GameTime.setSpeedState(GameTime.SPEED_STATE_FASTER);
                }
                else if (Input.GetKeyUp(KeyCode.Alpha3)) {
                    GameTime.setSpeedState(GameTime.SPEED_STATE_FASTEST);
                }
                else if (Input.GetKeyUp(KeyCode.Space)) {
                    GameTime.togglePause();
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

    /*
     * Selects the next Raycast2DHit in the array, making sure to rollover to index 0 if necessary
     * Returns the next hit, or null if selectionHits is not defined
     */
    private RaycastHit2D? nextSelectionHit() {
        if (selectionHits != null) {
            selectionHitsIndex++;
            if (selectionHitsIndex >= selectionHits.Length) selectionHitsIndex = 0;
            return selectionHits[selectionHitsIndex];
        }
        else {
            return null;
        }
    }

    /*
     * Resets selectionHits to the given hit array, also reseting the index
     */
    private void resetSelectionHits(RaycastHit2D[] hits) {
        selectionHitsIndex = 0;
        selectionHits = hits;
    }

    /*
     * Checks if two hit arrays are equal and valid, by ensuring they are defined, the same length, and contain
     * Pawns in the same order with the same unique name
     */
    private bool areSelectionHitsEqualAndValid(RaycastHit2D[] a, RaycastHit2D[] b) {
        if (a == null || b == null) return false;

        List<Pawn> apawns = convertSelectionHitsToPawnList(a);
        List<Pawn> bpawns = convertSelectionHitsToPawnList(b);

        if (apawns.Count == 0 || bpawns.Count == 0) return false;
        if (apawns.Count != bpawns.Count) return false;

        for (int i = 0; i < apawns.Count; i++) {
            if (!apawns[i].getName().Equals(bpawns[i].getName())) {
                return false;
            }
        }

        return true;
    }

    /*
     * Converts an array of hits to a list of pawns
     */
    private List<Pawn> convertSelectionHitsToPawnList(RaycastHit2D[] hits) {
        if (hits != null) {
            List<Pawn> pawns = new List<Pawn>();

            for (int i = 0; i < hits.Length; i++) {
                Pawn pawn = hits[i].transform.gameObject.GetComponent<PawnComponent>().pawn;
                if (pawn != null) {
                    pawns.Add(pawn);
                }
            }

            return pawns;
        }
        else {
            return null;
        }
    }
}
