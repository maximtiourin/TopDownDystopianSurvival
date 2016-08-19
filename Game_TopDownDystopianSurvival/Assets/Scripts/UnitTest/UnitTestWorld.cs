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
        ushort testtile = Tile.binaryToUshort("1010101001100110");
        ushort cmp = Tile.binaryToUshort("10101010");
        Assert.AreEqual(cmp, Tile.getTileId(testtile));

        //Tile.setTileId
        testtile = Tile.binaryToUshort("1010101001100110");
        ushort tileid = (ushort) 1;
        cmp = Tile.binaryToUshort("0000000101100110");
        Assert.AreEqual(cmp, Tile.setTileId(testtile, tileid));

        //Tile.getIsWall
        testtile = Tile.binaryToUshort("1010101001100110");
        Assert.IsFalse(Tile.getIsWall(testtile));
        testtile = Tile.binaryToUshort("1010101001100111");
        Assert.IsTrue(Tile.getIsWall(testtile));

        //Tile.setIsWall
        testtile = Tile.binaryToUshort("1010101001100110");
        Assert.IsFalse(Tile.getIsWall(Tile.setIsWall(testtile, false)));
        Assert.IsTrue(Tile.getIsWall(Tile.setIsWall(testtile, true)));

        /* ----------------------------------------------------------------------
         * ----------------------------------------------------------------------*/
    }
}
