using UnityEngine;
using UnityEngine.Assertions;
using System.Collections;

public class UnitTestWorld : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Assert.raiseExceptions = true;

        /* ----------------------------------------------------------------------
         * Tile
         * ----------------------------------------------------------------------*/

        //Tile.getTileId
        uint testtile = Tile.binaryToUInt("1010101001100110");
        uint cmp = Tile.binaryToUInt("10101010");
        Assert.AreEqual(cmp, Tile.getTileId(testtile));

        //Tile.setTileId
        testtile = Tile.binaryToUInt("1010101001100110");
        uint tileid = (uint) 1;
        cmp = Tile.binaryToUInt("0000000101100110");
        Assert.AreEqual(cmp, Tile.setTileId(testtile, tileid));

        //Tile.getIsWall
        testtile = Tile.binaryToUInt("1010101001100110");
        Assert.IsFalse(Tile.getIsWall(testtile));
        testtile = Tile.binaryToUInt("1010101001100111");
        Assert.IsTrue(Tile.getIsWall(testtile));

        //Tile.setIsWall
        testtile = Tile.binaryToUInt("1010101001100110");
        Assert.IsFalse(Tile.getIsWall(Tile.setIsWall(testtile, false)));
        Assert.IsTrue(Tile.getIsWall(Tile.setIsWall(testtile, true)));

        /* ----------------------------------------------------------------------
         * ----------------------------------------------------------------------*/
    }
}
