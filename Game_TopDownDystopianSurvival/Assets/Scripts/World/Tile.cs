using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Tile {
    private static uint tileid = 0;
    public static Dictionary<string, uint> tileids; //Mapping of string tile identifiers to numerical tile ids, expected to be in the range of (2^8 - 1)

    public static Dictionary<uint, TileData> tileData; //Mapping of numerical tile ids to structures containing all relevant information pertaining to that tile id

    public static bool generated = false;


    public static readonly uint maskTileID          =   binaryToUInt("11111111000000000000000000000000"); //8 bit tile id (0-255)
    public static readonly int  shiftTileID         =   24;
    public static readonly uint maskTileBitwise     =   binaryToUInt("00000000111100000000000000000000"); //4 bit neighbor description to enable selecting correct tile
    public static readonly int  shiftTileBitwise    =   20;
    public static readonly uint maskIsTileable      =   binaryToUInt("00000000000010000000000000000000"); //1 bit describing if this tile makes use of bitwise tileability
    public static readonly int  shiftIsTileable     =   19;
    public static readonly uint maskIsWall          =   binaryToUInt("00000000000000000000000000000001"); //1 bit describing if this tile is a floor (0), or a wall (1)
    public static readonly int  shiftIsWall         =   0;

    //Generate a unique tileid
    public static uint generateTileID() {
        uint id = tileid;
        tileid = (uint) (tileid + 1);
        return id;
    }

    public static uint getTileId(uint tile) {
        uint res = tile & maskTileID;
        res = res >> shiftTileID;
        return res;
    }

    public static uint setTileId(uint aTile, uint aTileid) {
        uint tile = aTile;
        uint tileid = aTileid;

        uint inverse = ~maskTileID;
        uint memory = tile & inverse;
        uint shift = tileid << shiftTileID;

        return shift | memory;
    }

    public static uint getTileBitwise(uint tile) {
        uint res = tile & maskTileBitwise;
        res = res >> shiftTileBitwise;
        return res;
    }

    public static uint setTileBitwise(uint aTile, uint abitwise) {
        uint tile = aTile;
        uint bitwise = abitwise;

        uint inverse = ~maskTileBitwise;
        uint memory = tile & inverse;
        uint shift = bitwise << shiftTileBitwise;

        return shift | memory;
    }

    public static bool getIsTileable(uint tile) {
        uint res = tile & maskIsTileable;
        res = res >> shiftIsTileable;
        return (res == 1) ? (true) : (false);
    }

    public static uint setIsTileable(uint aTile, bool isTileable) {
        uint tile = aTile;
        uint flag = (uint) ((isTileable) ? (1) : (0));

        uint inverse = ~maskIsTileable;
        uint memory = tile & inverse;
        uint shift = flag << shiftIsTileable;

        return shift | memory;
    }

    public static bool getIsWall(uint tile) {
        uint res = tile & maskIsWall;
        res = res >> shiftIsWall;
        return (res == 1) ? (true) : (false);
    }

    public static uint setIsWall(uint aTile, bool isWall) {
        uint tile = aTile;
        uint flag = (uint) ((isWall) ? (1) : (0));

        uint inverse = ~maskIsWall;
        uint memory = tile & inverse;
        uint shift = flag << shiftIsWall;

        return shift | memory;
    }

    public static uint binaryToUInt(string binary) {
        return System.Convert.ToUInt32(binary, 2);
    }

    public static TileData getTileDataForTileID(uint tileid) {
        return tileData[tileid];
    }

    public static uint nameToTileID(string name) {
        return tileids[name];
    }
}
