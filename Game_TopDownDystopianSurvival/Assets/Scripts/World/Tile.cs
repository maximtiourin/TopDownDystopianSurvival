using UnityEngine;
using System.Collections;

public static class Tile {
    /*
     | Tile bit structure (ushort)
     | 0000     0000    0000    0000
     |                             *    0 - floor, 1 - wall
     |                            *     
     | ****     ****                    8-bit tile-id identifier for tile type (grass, dirt, gravel, etc) 0-255
     */
    public static readonly ushort maskIsWall = 1;       // (0000 0000 0000 0001)
    public static readonly byte shiftIsWall = 0;
    public static readonly ushort maskTileId = 65280;   // (1111 1111 0000 0000) Masks out only the tileid bits from the ushort (still needs to be bitshifted to attain the id value)
    public static readonly byte shiftTileId = 8;

    public static ushort getTileId(ushort tile) {
        ushort res = (ushort) (tile & maskTileId);
        res = (ushort) (res >> shiftTileId);
        return res;
    }

    public static ushort setTileId(ushort aTile, ushort aTileid) {
        ushort tile = aTile;
        ushort tileid = aTileid;

        ushort inverse = (ushort) (~maskTileId);
        ushort memory = (ushort) (tile & inverse);
        ushort shift = (ushort) (tileid << shiftTileId);

        return (ushort) (shift | memory);
    }

    public static bool getIsWall(ushort tile) {
        ushort res = (ushort) (tile & maskIsWall);
        res = (ushort) (res >> shiftIsWall);
        return (res == 1) ? (true) : (false);
    }
}
