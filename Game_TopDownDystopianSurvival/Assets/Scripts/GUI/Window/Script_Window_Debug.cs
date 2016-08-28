using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Script_Window_Debug : MonoBehaviour {
    public GameObject levelMouseContainer;
    public GameObject levelContainer;
    public GameObject debugOverlayContainer;
    public Text text;
    private Script_World_Mouse_LevelInteraction levelMouse;
    private Level level;
    private DebugLevelOverlay debugOverlay;

	// Use this for initialization
	void Start () {
        levelMouse = levelMouseContainer.GetComponent<Script_World_Mouse_LevelInteraction>();
        level = levelContainer.GetComponent<Level>();
        debugOverlay = debugOverlayContainer.GetComponent<DebugLevelOverlay>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (level != null && level.isLoaded()) {
            string n = "\n";
            string c = ", ";

            int tx = levelMouse.getTileX();
            int ty = levelMouse.getTileY();
            float mx = levelMouse.getMouseX();
            float my = levelMouse.getMouseY();

            string str = "";

            str += "FixedUpdate() Tick: " + level.getTick() + n;
            str += "Time Elapsed: " + level.getElapsedTime() + n;

            str += "Mouse Tile X: " + tx + n;
            str += "Mouse Tile Y: " + ty + n;
            str += "Mouse X: " + mx + n;
            str += "Mouse Y: " + my + n;

            uint tile = level.getTileAtPosition(tx, ty);

            uint tileid = Tile.getTileId(tile);
            bool isWall = Tile.getIsWall(tile);
            bool isTileable = Tile.getIsTileable(tile);
            uint bitwise = Tile.getTileBitwise(tile);
            uint cornerbitwise = Tile.getTileCornerBitwise(tile);

            TileData tiledata = Tile.getTileDataForTileID(tileid);
            string tilename = tiledata.name;

            str += "Tileset: " + tilename + n;
            str += "Tileid: " + tileid + n;
            str += "isWall: " + isWall + n;
            str += "isTileable: " + isTileable + n;
            str += "Bitwise: " + bitwise + n;
            str += "CornerBitwise: " + cornerbitwise + n;

            Region region = level.getRegionAtPosition(tx, ty);

            int rx = level.getInnerRegionXAtPositionX(tx);
            int ry = level.getInnerRegionYAtPositionY(ty);

            Node node = region.getNodeAtNodePosition(rx, ry);

            str += "Region X: " + region.getColumn() + n;
            str += "Region Y: " + region.getRow() + n;
            if (node != null) {
                str += "Region Node X: " + node.x + n;
                str += "Region Node Y: " + node.y + n;
                str += "Region Node FillID: " + node.fillID + n;
            }

            /*DebugLevelOverlay dbo = debugOverlay;
            if (dbo.region != null) {
                str += "DebugOverlay Region X: " + dbo.region.getColumn() + n;
                str += "DebugOverlay Region Y: " + dbo.region.getRow() + n;
            }
            if (dbo.chunk != null) {
                str += "DebugOverlay Chunk FillID: " + dbo.chunk.fillID + n;
            }
            if (dbo.cacheRegion != null) {
                str += "DebugOverlay cacheRegion X: " + dbo.cacheRegion.getColumn() + n;
                str += "DebugOverlay cacheRegion Y: " + dbo.cacheRegion.getRow() + n;
            }
            if (dbo.cacheChunk != null) {
                str += "DebugOverlay cacheChunk FillID: " + dbo.cacheChunk.fillID + n;
            }*/

            if (levelMouse.isObstructed()) str += "Mouse over GUI" + n;

            setDataText(str);
        }
	}

    private void setDataText(string str) {
        if (text != null) {
            text.text = str;
        }
    }

    public void toggleWindowIsActive() {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}
