using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Script_Window_Debug : MonoBehaviour {
    public GameObject levelMouseContainer;
    public GameObject levelContainer;
    public Text text;
    private Script_World_Mouse_LevelInteraction levelMouse;
    private Level level;

	// Use this for initialization
	void Start () {
        levelMouse = levelMouseContainer.GetComponent<Script_World_Mouse_LevelInteraction>();
        level = levelContainer.GetComponent<Level>();
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

            str += "Tick: " + level.getTick() + n;

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

            if (levelMouse.isObstructed()) str += "Mouse over GUI" + n;

            setDataText(str);
        }
	}

    private void setDataText(string str) {
        if (text != null) {
            text.text = str;
        }
    }
}
