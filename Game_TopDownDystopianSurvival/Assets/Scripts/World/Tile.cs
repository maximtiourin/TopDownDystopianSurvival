using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Tile {
    private static ushort tileid = 0;
    public static Dictionary<string, ushort> tileids; //Mapping of string tile identifiers to numerical tile ids, expected to be in the range of (2^8 - 1)

    public static Dictionary<ushort, TileData> tileData;

    public static bool generated = false;

    /*
     | Tile bit structure (ushort)
     | 0000     0000    0000    0000
     |                             *    0 - floor, 1 - wall
     |                            *     TODO
     | ****     ****                    8-bit tile-id identifier for tile type (grass, dirt, gravel, etc) 0-255
     */
    public static readonly ushort       maskIsWall      =   1;
    public static readonly byte         shiftIsWall     =   0;
    public static readonly ushort       maskTileID      =   binaryToUShort("1111111100000000");
    public static readonly byte         shiftTileID     =   8;

    public static ushort generateTileID() {
        ushort id = tileid;
        tileid = (ushort) (tileid + 1);
        return id;
    }

    public static ushort getTileId(ushort tile) {
        ushort res = (ushort) (tile & maskTileID);
        res = (ushort) (res >> shiftTileID);
        return res;
    }

    public static ushort setTileId(ushort aTile, ushort aTileid) {
        ushort tile = aTile;
        ushort tileid = aTileid;

        ushort inverse = (ushort) (~maskTileID);
        ushort memory = (ushort) (tile & inverse);
        ushort shift = (ushort) (tileid << shiftTileID);

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

    public static ushort binaryToUShort(string binary) {
        return System.Convert.ToUInt16(binary, 2);
    }

    public static uint binaryToUInt(string binary) {
        return System.Convert.ToUInt32(binary, 2);
    }

    public static TileData getTileDataForTileID(ushort tileid) {
        return tileData[tileid];
    }

    public static ushort nameToTileID(string name) {
        return tileids[name];
    }
}
