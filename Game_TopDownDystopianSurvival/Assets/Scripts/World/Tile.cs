using System.Collections;

public static class Tile {
    /*
     | Tile bit structure (ushort)
     | 0000     0000    0000    0000
     |                             *    0 - floor, 1 - wall
     |                            *     TODO
     | ****     ****                    8-bit tile-id identifier for tile type (grass, dirt, gravel, etc) 0-255
     */
    public static readonly ushort       maskIsWall      =   1;
    public static readonly byte         shiftIsWall     =   0;
    public static readonly ushort       maskTileId      =   binaryToUshort("1111111100000000");
    public static readonly byte         shiftTileId     =   8;

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

    public static ushort setIsWall(ushort aTile, bool isWall) {
        ushort tile = aTile;
        ushort flag = (ushort) ((isWall) ? (1) : (0));

        ushort inverse = (ushort) (~maskIsWall);
        ushort memory = (ushort) (tile & inverse);
        ushort shift = (ushort) (flag << shiftIsWall);

        return (ushort) (shift | memory);
    }

    public static ushort binaryToUshort(string binary) {
        return System.Convert.ToUInt16(binary, 2);
    }
}
