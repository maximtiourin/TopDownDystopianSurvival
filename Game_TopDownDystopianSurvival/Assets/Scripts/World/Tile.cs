using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class Tile {
    private static string ERROR = "!key_not_found";

    private static uint tileid = 0;
    public static Dictionary<string, uint> tileids; //Mapping of string tile identifiers to numerical tile ids, expected to be in the range of (2^8 - 1)
    public static Dictionary<uint, TileData> tileData; //Mapping of numerical tile ids to structures containing all relevant information pertaining to that tile id
    public static Dictionary<uint, Dictionary<uint, Sprite>> dynamicFillSprites; //Mapping of tileid to a hashmap that maps bitwise and corner bitwise to a dynamically generated sprite

    public static bool generated = false;


    public static readonly uint maskTileID              =   binaryToUInt("11111111000000000000000000000000"); //8 bit tile id (0-255)
    public static readonly int  shiftTileID             =   24;
    public static readonly uint maskTileBitwise         =   binaryToUInt("00000000111100000000000000000000"); //4 bit neighbor description to enable selecting correct tile
    public static readonly int  shiftTileBitwise        =   20;
    public static readonly uint maskTileCornerBitwise   =   binaryToUInt("00000000000011110000000000000000"); //4 bit corner neighbor description to enable selecting correct fill tile
    public static readonly int  shiftTileCornerBitwise  =   16;
    public static readonly uint maskIsTileable          =   binaryToUInt("00000000000000001000000000000000"); //1 bit describing if this tile makes use of bitwise tileability
    public static readonly int  shiftIsTileable         =   15;
    public static readonly uint maskIsWall              =   binaryToUInt("00000000000000000000000000000001"); //1 bit describing if this tile is a floor (0), or a wall (1)
    public static readonly int  shiftIsWall             =   0;

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

    public static uint getTileCornerBitwise(uint tile) {
        uint res = tile & maskTileCornerBitwise;
        res = res >> shiftTileCornerBitwise;
        return res;
    }

    public static uint setTileCornerBitwise(uint aTile, uint abitwise) {
        uint tile = aTile;
        uint bitwise = abitwise;

        uint inverse = ~maskTileCornerBitwise;
        uint memory = tile & inverse;
        uint shift = bitwise << shiftTileCornerBitwise;

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
        if (tileids.ContainsKey(name)) {
            return tileids[name];
        }
        else {
            return tileids[ERROR];
        }
    }

    //Caches any generated sprites after they are created for the first time
    public static Sprite createDynamicFillSprite(uint tileid, Sprite baseSprite, Color fillColor, uint bitwise, uint cornerbitwise) {
        if (cornerbitwise > 0) {
            Texture2D baseTex = baseSprite.texture;
            Rect baseRect = baseSprite.rect;

            Dictionary<uint, Sprite> map;
            if (!dynamicFillSprites.ContainsKey(tileid)) {
                map = new Dictionary<uint, Sprite>();
                dynamicFillSprites[tileid] = map;
            }
            else {
                map = dynamicFillSprites[tileid];
            }

            uint dynamicKey = getDynamicBitwiseKey(bitwise, cornerbitwise);

            if (map.ContainsKey(dynamicKey)) {
                //Return cached sprite
                return map[dynamicKey];
            }
            else {
                //Create appropriate Texture
                Texture2D newTex = copyTexture2D(baseTex, baseRect);

                int w = newTex.width;
                int h = newTex.height;
                int hx = w / 2;     // 1/2th width
                int hhx = hx / 2;   // 1/4th width
                int hy = h / 2;     // 1/2th height
                int hhy = hy / 2;   // 1/4th height

                if ((cornerbitwise & 1) == 1) {
                    for (int x = 0; x < hx; x++) {
                        for (int y = h - hhy; y < h; y++) {
                            newTex.SetPixel(x, y, fillColor);
                        }
                    }
                }
                if ((cornerbitwise & 2) == 2) {
                    for (int x = 0; x < hx; x++) {
                        for (int y = 0; y < hy; y++) {
                            newTex.SetPixel(x, y, fillColor);
                        }
                    }
                }
                if ((cornerbitwise & 4) == 4) {
                    for (int x = hx; x < w; x++) {
                        for (int y = 0; y < hy; y++) {
                            newTex.SetPixel(x, y, fillColor);
                        }
                    }
                }
                if ((cornerbitwise & 8) == 8) {
                    for (int x = hx; x < w; x++) {
                        for (int y = h - hhy; y < h; y++) {
                            newTex.SetPixel(x, y, fillColor);
                        }
                    }
                }

                newTex.Apply();

                //Create sprite and return
                Sprite sprite = Sprite.Create(newTex, new Rect(0, 0, newTex.width, newTex.height), new Vector2(0, 0), newTex.height);

                map[dynamicKey] = sprite;

                return sprite;
            }
        }
        else {
            return baseSprite;
        }
    }

    private static Texture2D copyTexture2D(Texture2D baseTex, Rect baseRect) {
        int xoff = (int) baseRect.x;
        int yoff = (int) baseRect.y;
        int w = (int) baseRect.width;
        int h = (int) baseRect.height;

        Texture2D newTex = new Texture2D((int) baseRect.width, (int) baseRect.height);
        newTex.filterMode = FilterMode.Point;

        Color[] pixels = baseTex.GetPixels(xoff, yoff, w, h);

        newTex.SetPixels(pixels);

        newTex.Apply();

        return newTex;
    }

    private static uint getDynamicBitwiseKey(uint bitwise, uint cornerbitwise) {
        return bitwise | (cornerbitwise << 4);
    }
}
