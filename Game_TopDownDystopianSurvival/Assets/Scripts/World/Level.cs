using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour {
    private int width = 256;
    private int height = 256;

    public static readonly int regionWidth = 16;
    public static readonly int regionHeight = 16;

    private int regionColumns;
    private int regionRows;

    private Region[,] regions;

    private Dictionary<ulong, Chunk> chunkMap; //Hashmap of generated chunks

    private ulong chunkGUID; //The global unique identifier counter for generated chunks.

    private ushort[,] tiles;

	// Use this for initialization
	void Start () {
        regionColumns = width / regionWidth;
        regionRows = height / regionHeight;

        regions = new Region[regionColumns, regionRows];
        initRegions();

        chunkMap = new Dictionary<ulong, Chunk>();

        chunkGUID = 0;

        tiles = new ushort[width, height];
	}
	
    //TODO
	// Update is called once per frame
	void Update () {
	
	}

    //TODO
    public bool isObstacleAtGridPosition(int x, int y) {
        //TODO for now only consider tile walls as obstacles
        return Tile.getIsWall(tiles[x, y]);
    }

    public bool isObstacleAtRegionPosition(Region region, int x, int y) {
        return isObstacleAtGridPosition((region.getWidth() * region.getColumn()) + x, (region.getHeight() * region.getRow()) + y);
    }

    //TODO
    public void generateLevel() {
        //TODO Generate Tiles --Temporary gen
        

        //TODO Generate all Entities

        //Generate Regions

    }

    public void initRegions() {
        for (int c = 0; c < regionColumns; c++) {
            for (int r = 0; r < regionRows; r++) {
                regions[c, r] = new Region(this, regionWidth, regionHeight, c, r);
            }
        }
    }

    public Dictionary<ulong, Chunk> getChunkMap() {
        return chunkMap;
    }

    public ulong generateChunkGUID() {
        return chunkGUID++;
    }
}
