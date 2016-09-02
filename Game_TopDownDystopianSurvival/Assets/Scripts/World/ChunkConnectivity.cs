using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChunkConnectivity {
    public static readonly uint maskXPosition   =   binaryToUInt("00111111111111000000000000000000"); //12 bit level x start position (0-4095)
    public static readonly int  shiftXPosition  =   18;
    public static readonly uint maskYPosition   =   binaryToUInt("00000000000000111111111111000000"); //12 bit level y start position (0-4095)
    public static readonly int  shiftYPosition  =   6;
    public static readonly uint maskLength      =   binaryToUInt("00000000000000000000000000111110"); //5 bit level length (0-31)
    public static readonly int  shiftLength     =   1;
    public static readonly uint maskConfig      =   binaryToUInt("00000000000000000000000000000001"); //1 bit level config (0-1) - 0 horizontal, 1 vertical
    public static readonly int  shiftConfig     =   0;

    public enum Configuration {
        Horizontal, Vertical
    }

    private Dictionary<uint, List<Chunk>> map;

    public ChunkConnectivity() {
        map = new Dictionary<uint, List<Chunk>>();
    }

    //Inserts the chunk into the list mapped to the given key, if the chunk is not already a part of that list.
    //Creates a new list if the key is unique. Returns true/false based on success of addition
    public bool connectChunk(uint key, Chunk value) {
        List<Chunk> list;

        if (map.ContainsKey(key)) {
            list = map[key];
        }
        else {
            list = new List<Chunk>();
            map[key] = list;
        }

        if (!list.Contains(value)) {
            list.Add(value);
            value.addConnection(key);
            return true;
        }
        else {
            return false;
        }
    }

    //Attempts to remove the chunk in the list mapped to the given key, if it exists, and returns true/false based on success of removal.
    public bool disconnectChunk(uint key, Chunk value) {
        value.removeConnection(key);

        if (map.ContainsKey(key)) {
            List<Chunk> list = map[key];

            return list.Remove(value);
        }
        else {
            return false;
        }
    }

    public void Clear() {
        map.Clear();
    }

    public static bool isPositionContainedByConnectivityHash(uint hash, int x, int y) {
        int cx = getXPosition(hash);
        int cy = getYPosition(hash);
        int len = getLength(hash);
        Configuration config = getConfig(hash);

        if (config == Configuration.Horizontal) {
            return (y == cy) && (x >= cx && x < cx + len);
        }
        else {
            return (x == cx) && (y >= cy && y < cy + len);
        }
    }

    public static uint generateConnectivityHash(int x, int y, int length, Configuration config) {
        uint res = 0;
        res = setXPosition(res, x);
        res = setYPosition(res, y);
        res = setLength(res, length);
        res = setConfig(res, config);
        return res;
    }

    public static uint setXPosition(uint hash, int xpos) {
        uint x = (uint) xpos;

        uint inverse = ~maskXPosition;
        uint memory = hash & inverse;
        uint shift = x << shiftXPosition;

        return shift | memory;
    }

    public static int getXPosition(uint hash) {
        uint res = hash & maskXPosition;
        res = res >> shiftXPosition;
        return (int) res;
    }

    public static uint setYPosition(uint hash, int ypos) {
        uint y = (uint) ypos;

        uint inverse = ~maskYPosition;
        uint memory = hash & inverse;
        uint shift = y << shiftYPosition;

        return shift | memory;
    }

    public static int getYPosition(uint hash) {
        uint res = hash & maskYPosition;
        res = res >> shiftYPosition;
        return (int) res;
    }

    public static uint setLength(uint hash, int length) {
        uint len = (uint) length;

        uint inverse = ~maskLength;
        uint memory = hash & inverse;
        uint shift = len << shiftLength;

        return shift | memory;
    }

    public static int getLength(uint hash) {
        uint res = hash & maskLength;
        res = res >> shiftLength;
        return (int) res;
    }

    public static uint setConfig(uint hash, Configuration config) {
        uint c = (uint)  ((config == Configuration.Horizontal) ? (0) : (1));

        uint inverse = ~maskConfig;
        uint memory = hash & inverse;
        uint shift = c << shiftConfig;

        return shift | memory;
    }

    public static Configuration getConfig(uint hash) {
        uint res = hash & maskConfig;
        res = res >> shiftConfig;
        return ((res == 0) ? (Configuration.Horizontal) : (Configuration.Vertical));
    }

    public static uint binaryToUInt(string binary) {
        return System.Convert.ToUInt32(binary, 2);
    }
}
