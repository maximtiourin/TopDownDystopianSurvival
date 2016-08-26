﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour, Loadable {
    private int width = 256;
    private int height = 256;

    public static readonly int regionWidth = 16;
    public static readonly int regionHeight = 16;

    private int regionColumns;
    private int regionRows;

    private Region[,] regions;

    private uint[,] tiles;

    private GameObject renderTilesContainer;
    private GameObject[,] renderTiles;

    private bool loaded;

	// Use this for initialization
	void Start () {
        regionColumns = width / regionWidth;
        regionRows = height / regionHeight;

        regions = new Region[regionColumns, regionRows];
        initRegions();

        tiles = new uint[width, height];

        renderTilesContainer = null;
        renderTiles = new GameObject[width, height];

        loaded = false;
    }
	
    //TODO
	// Update is called once per frame
	void Update () {
        if (isLoaded()) {
            //Do frame dependent logic
        }
	}

    void FixedUpdate() {
        if (isLoaded()) {
            //Do timestep dependent logic
        }
    }

    //TODO
    public bool isObstacleAtGridPosition(int x, int y) {
        //TODO for now only consider tile walls as obstacles
        return Tile.getIsWall(tiles[x, y]);
    }

    public bool isObstacleAtRegionPosition(Region region, int x, int y) {
        return isObstacleAtGridPosition((region.getWidth() * region.getColumn()) + x, (region.getHeight() * region.getRow()) + y);
    }

    public bool isValidTilePosition(int x, int y) {
        return ((x >= 0 && x < width) && (y >= 0 && y < height));
    }

    //TODO
    private void generateLevel() {
        //TODO Generate Tiles --Temporary gen        
        generateTiles();

        //TODO Generate all Entities

        //Generate Regions

    }

    //TODO testing generation currently
    private void generateTiles() {
        uint tile;

        //TODO Tile Cache - Eventually implement a more global tile caching system
        TileData test01 = Tile.getTileDataForTileID(Tile.nameToTileID("test01"));
        TileData test02 = Tile.getTileDataForTileID(Tile.nameToTileID("test02"));

        //Floors
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                tile = tiles[x, y];
                tile = Tile.setIsWall(tile, false);
                tile = Tile.setTileId(tile, test01.tileid);
                tile = Tile.setIsTileable(tile, test01.isTileable);
                tiles[x, y] = tile;
            }
        }

        //Walls
        for (int x = (width / 2) - (width / 4); x < (width / 2) + (width / 4); x++) {
            tile = tiles[x, height / 2];
            tile = Tile.setIsWall(tile, true);
            tile = Tile.setTileId(tile, test02.tileid);
            tile = Tile.setIsTileable(tile, test02.isTileable);
            tiles[x, height / 2] = tile;
        }

        tile = tiles[0, 0];
        tile = Tile.setIsWall(tile, true);
        tile = Tile.setTileId(tile, test02.tileid);
        tile = Tile.setIsTileable(tile, test02.isTileable);
        tiles[0, 0] = tile;
        tile = tiles[1, 1];
        tile = Tile.setIsWall(tile, true);
        tile = Tile.setTileId(tile, test02.tileid);
        tile = Tile.setIsTileable(tile, test02.isTileable);
        tiles[1, 1] = tile;
        tile = tiles[1, 0];
        tile = Tile.setIsWall(tile, true);
        tile = Tile.setTileId(tile, test02.tileid);
        tile = Tile.setIsTileable(tile, test02.isTileable);
        tiles[1, 0] = tile;
    }

    //Updates tiling for all tiles, very expensive call, instead update tiling for individual tiles or clusters when possible
    private void calculateTiling() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                updateTilingForTile(x, y);
            }
        }
    }

    public void updateTilingForTile(int x, int y) {
        if (isValidTilePosition(x, y)) {
            uint tile = tiles[x, y];

            tile = Tile.setTileBitwise(tile, 0);
            tile = Tile.setTileCornerBitwise(tile, 0);

            tiles[x, y] = tile;

            if (Tile.getIsTileable(tile)) {
                bool up = y + 1 < height;
                bool left = x - 1 >= 0;
                bool down = y - 1 >= 0;
                bool right = x + 1 < width;

                /*
                 * Check Regular Neighbors
                 */
                if (up) {
                    uint cmp = tiles[x, y + 1];

                    if (isTilingCompatible(tile, cmp)) {
                        uint tilebit = Tile.getTileBitwise(tile);
                        tilebit = tilebit | 1;
                        tile = Tile.setTileBitwise(tile, tilebit);
                        tiles[x, y] = tile;
                    }
                }
                if (down) {
                    uint cmp = tiles[x, y - 1];

                    if (isTilingCompatible(tile, cmp)) {
                        uint tilebit = Tile.getTileBitwise(tile);
                        tilebit = tilebit | 4;
                        tile = Tile.setTileBitwise(tile, tilebit);
                        tiles[x, y] = tile;
                    }
                }
                if (left) {
                    uint cmp = tiles[x - 1, y];

                    if (isTilingCompatible(tile, cmp)) {
                        uint tilebit = Tile.getTileBitwise(tile);
                        tilebit = tilebit | 2;
                        tile = Tile.setTileBitwise(tile, tilebit);
                        tiles[x, y] = tile;
                    }
                }
                if (right) {
                    uint cmp = tiles[x + 1, y];

                    if (isTilingCompatible(tile, cmp)) {
                        uint tilebit = Tile.getTileBitwise(tile);
                        tilebit = tilebit | 8;
                        tile = Tile.setTileBitwise(tile, tilebit);
                        tiles[x, y] = tile;
                    }
                }

                /*
                 * Check Corners only if needed
                 */
                uint bitwise = Tile.getTileBitwise(tile);
                if ((bitwise & 3) == 3 && up && left) {
                    uint cmp = tiles[x - 1, y + 1];

                    if (isTilingCompatible(tile, cmp)) {
                        uint cornerbit = Tile.getTileCornerBitwise(tile);
                        cornerbit = cornerbit | 1;
                        tile = Tile.setTileCornerBitwise(tile, cornerbit);
                        tiles[x, y] = tile;
                    }
                }
                if ((bitwise & 6) == 6 && down && left) {
                    uint cmp = tiles[x - 1, y - 1];

                    if (isTilingCompatible(tile, cmp)) {
                        uint cornerbit = Tile.getTileCornerBitwise(tile);
                        cornerbit = cornerbit | 2;
                        tile = Tile.setTileCornerBitwise(tile, cornerbit);
                        tiles[x, y] = tile;
                    }
                }
                if ((bitwise & 12) == 12 && down && right) {
                    uint cmp = tiles[x + 1, y - 1];

                    if (isTilingCompatible(tile, cmp)) {
                        uint cornerbit = Tile.getTileCornerBitwise(tile);
                        cornerbit = cornerbit | 4;
                        tile = Tile.setTileCornerBitwise(tile, cornerbit);
                        tiles[x, y] = tile;
                    }
                }
                if ((bitwise & 9) == 9 && up && right) {
                    uint cmp = tiles[x + 1, y + 1];

                    if (isTilingCompatible(tile, cmp)) {
                        uint cornerbit = Tile.getTileCornerBitwise(tile);
                        cornerbit = cornerbit | 8;
                        tile = Tile.setTileCornerBitwise(tile, cornerbit);
                        tiles[x, y] = tile;
                    }
                }
            }
        }
    }

    //A cluster is the tile located at x,y as well as the neighbor corners and tiles up, down, left, and right of that tile
    public void updateTilingForTileClusterAtPosition(int x, int y) {
        updateTilingForTile(x - 1, y + 1);
        updateTilingForTile(x, y + 1);
        updateTilingForTile(x + 1, y + 1);
        updateTilingForTile(x + 1, y);
        updateTilingForTile(x + 1, y - 1);
        updateTilingForTile(x, y - 1);
        updateTilingForTile(x - 1, y - 1);
        updateTilingForTile(x - 1, y);
        updateTilingForTile(x, y);
    }

    //TODO handles all tileables cases to make sure that two tileable tiles actually want to tile with each other
    private bool isTilingCompatible(uint tileA, uint tileB) {
        if (Tile.getIsWall(tileA) && Tile.getIsWall(tileB)) {
            return true;
        }
        else {
            return false;
        }
    }

    public void createFloorTileAtPosition(int x, int y, uint tileid) {
        if (isValidTilePosition(x, y)) {
            uint tile = 0;

            TileData data = Tile.getTileDataForTileID(tileid);

            tile = Tile.setTileId(tile, tileid);
            tile = Tile.setIsTileable(tile, data.isTileable);
            tile = Tile.setIsWall(tile, false);

            tiles[x, y] = tile;

            updateTilingForTileClusterAtPosition(x, y);

            updateRenderTilesForTileClusterAtPosition(x, y);            
        }
    }

    public void createFloorTileAtPosition(int x, int y, string floortype) {
        createFloorTileAtPosition(x, y, Tile.nameToTileID(floortype));
    }

    public void createWallTileAtPosition(int x, int y, uint tileid) {
        if (isValidTilePosition(x, y)) {
            uint tile = 0;

            TileData data = Tile.getTileDataForTileID(tileid);

            tile = Tile.setTileId(tile, tileid);
            tile = Tile.setIsTileable(tile, data.isTileable);
            tile = Tile.setIsWall(tile, true);

            tiles[x, y] = tile;

            updateTilingForTileClusterAtPosition(x, y);

            updateRenderTilesForTileClusterAtPosition(x, y);
        }
    }

    public void createWallTileAtPosition(int x, int y, string walltype) {
        createWallTileAtPosition(x, y, Tile.nameToTileID(walltype));
    }

    public void updateRenderTileAtPosition(int x, int y) {
        if (isValidTilePosition(x, y)) {
            GameObject renderTile = renderTiles[x, y];
            SpriteRenderer rend = renderTile.GetComponent<SpriteRenderer>();

            uint tile = tiles[x, y];

            uint tileid = Tile.getTileId(tile);
            uint bitwise = Tile.getTileBitwise(tile);
            uint cornerbitwise = Tile.getTileCornerBitwise(tile);

            TileData data = Tile.getTileDataForTileID(tileid);

            if (data.isTileable) {
                rend.sprite = Tile.createDynamicFillSprite(tileid, data.sprites[bitwise], data.fillColor, bitwise, cornerbitwise);
            }
            else {
                rend.sprite = data.sprite;
            }

            rend.sharedMaterial = data.material;
        }
    }

    //A cluster is the tile located at x,y as well as the neighbor corners and tiles up, down, left, and right of that tile
    public void updateRenderTilesForTileClusterAtPosition(int x, int y) {
        updateRenderTileAtPosition(x - 1, y + 1);
        updateRenderTileAtPosition(x, y + 1);
        updateRenderTileAtPosition(x + 1, y + 1);
        updateRenderTileAtPosition(x + 1, y);
        updateRenderTileAtPosition(x + 1, y - 1);
        updateRenderTileAtPosition(x, y - 1);
        updateRenderTileAtPosition(x - 1, y - 1);
        updateRenderTileAtPosition(x - 1, y);
        updateRenderTileAtPosition(x, y);
    }

    /*
     * Freshly generates and stores references to new RenderTile gameobjects
     * which are in charge of determining what should be rendered for the level in a given tile.
     * This should only be called on the initial loading of a level. Individual render tiles should be
     * modified after that when needed.
     */
    private void generateRenderTiles() {
        renderTilesContainer = new GameObject();
        renderTilesContainer.name = "RenderTileContainer";
        renderTilesContainer.transform.parent = this.transform;

        //TODO Create tiles (eventually need to take into account multiple drawing techniques to properly layer tiles, and only apply relevant textures/materials/etc
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GameObject renderTile = new GameObject();
                SpriteRenderer rend = renderTile.AddComponent<SpriteRenderer>();

                renderTile.name = "RenderTile_" + x + "_" + y;

                renderTile.transform.parent = renderTilesContainer.transform;

                renderTile.transform.localPosition = new Vector3(x, y, 0f);

                renderTiles[x, y] = renderTile;

                updateRenderTileAtPosition(x, y);
            }
        }
    }

    public uint getTileAtPosition(int x, int y) {
        return tiles[x, y];
    }

    private void initRegions() {
        for (int c = 0; c < regionColumns; c++) {
            for (int r = 0; r < regionRows; r++) {
                regions[c, r] = new Region(this, regionWidth, regionHeight, c, r);
            }
        }
    }

    public void load() {
        generateLevel();
        calculateTiling();
        generateRenderTiles();

        loaded = true;
    }

    public bool isLoaded() {
        return loaded;
    }

    public int getWidth() {
        return width;
    }

    public int getHeight() {
        return height;
    }
}
