﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Level : MonoBehaviour, Loadable {
    //TODO - DEBUG TEMPORARY VARIABLE FOR QUICK TESTING
    public Sprite TestCharacterSprite;

    private int width = 256;
    private int height = 256;

    public static readonly int regionWidth = 16;
    public static readonly int regionHeight = 16;

    private int regionColumns;
    private int regionRows;

    private Region[,] regions;

    private uint[,] tiles;

    private ChunkConnectivity chunkConnections;

    private GameObject renderTilesContainer;
    private GameObject[,] renderTiles;

    private GameObject pawnObjectContainer;
    private HashSet<Pawn> pawnBatchOwnershipSet; //Current Set of pawns which need their region/chunk ownership recalculated

    private bool loaded;

	// Use this for initialization
	void Start () {
        regionColumns = width / regionWidth;
        regionRows = height / regionHeight;

        regions = new Region[regionColumns, regionRows];
        initRegions();

        tiles = new uint[width, height];

        chunkConnections = new ChunkConnectivity();

        renderTilesContainer = null;
        renderTiles = new GameObject[width, height];

        pawnObjectContainer = null;
        pawnBatchOwnershipSet = new HashSet<Pawn>();

        loaded = false;
    }
	
    //TODO
	// Update is called once per frame
	void Update () {
        if (isLoaded()) {

        }
	}

    void FixedUpdate() {
        if (isLoaded()) {

        }
    }

    //TODO for now only consider tile walls as obstacles
    public bool isObstacleAtGridPosition(int x, int y) {
        return Tile.getIsWall(tiles[x, y]);
    }

    public bool isObstacleAtRegionPosition(Region region, int x, int y) {
        return isObstacleAtGridPosition(getPositionXAtInnerRegionX(region, x), getPositionYAtInnerRegionY(region, y));
    }

    //TODO for now consider a door just a tile with the testdoor tileset applied
    public bool isDoorAtGridPosition(int x, int y) {
        return Tile.getTileId(tiles[x, y]) == Tile.nameToTileID("testdoor");
    }

    public bool isDoorAtRegionPosition(Region region, int x, int y) {
        return isDoorAtGridPosition(getPositionXAtInnerRegionX(region, x), getPositionYAtInnerRegionY(region, y));
    }

    public bool isValidTilePosition(int x, int y) {
        return ((x >= 0 && x < width) && (y >= 0 && y < height));
    }

    //Checks if the region row and column are within bounds
    public bool isValidRegionPosition(int c, int r) {
        return ((c >= 0 && c < regionColumns) && (r >= 0 && r < regionRows));
    }

    //Checks if the region inner position converted to level position is within the level bounds
    public bool isValidRegionInnerPosition(Region region, int x, int y) {
        int rx = getPositionXAtInnerRegionX(region, x);
        int ry = getPositionYAtInnerRegionY(region, y);
        return isValidTilePosition(rx, ry);
    }

    private void generateLevel() {
        //Generate Tiles and first pass calculate their tiling  
        generateTiles();
        calculateTiling();

        //Create RenderTile objects and first pass determine their sprite
        generateRenderTiles();

        //First pass Calculate Regions
        calculateRegions();
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

    //Calculates all regions, very expensive call, instead update individual regions when possible
    private void calculateRegions() {
        for (int c = 0; c < regionColumns; c++) {
            for (int r = 0; r < regionRows; r++) {
                calculateRegion(c, r, true);
            }
        }

        calculatePawnBatchOwnership();
    }

    private void calculateRegion(int column, int row, bool batch = false) {
        regions[column, row].calculateRegion();

        if (!batch) calculatePawnBatchOwnership();
    }

    //Recalculates the region the tile is in, as well as any bordering regions if they might be affected
    private void recalculateRegionsAroundPosition(int x, int y) {
        int rtx = getInnerRegionXAtPositionX(x);
        int rty = getInnerRegionYAtPositionY(y);

        bool left = rtx == 0;
        bool right = rtx == regionWidth - 1;
        bool up = rty == regionHeight - 1;
        bool down = rty == 0;

        int c = getRegionXAtPositionX(x);
        int r = getRegionYAtPositionY(y);

        recalculateRegion(c, r, true);

        if (left) {
            recalculateRegion(c - 1, r, true);
        }
        if (right) {
            recalculateRegion(c + 1, r, true);
        }
        if (up) {
            recalculateRegion(c, r + 1, true);
        }
        if (down) {
            recalculateRegion(c, r - 1, true);
        }
        if (left && up) {
            recalculateRegion(c - 1, r + 1, true);
        }
        if (right && up) {
            recalculateRegion(c + 1, r + 1, true);
        }
        if (left && down) {
            recalculateRegion(c - 1, r - 1, true);
        }
        if (right && down) {
            recalculateRegion(c + 1, r - 1, true);
        }

        calculatePawnBatchOwnership();
    }

    private void recalculateRegion(int c, int r, bool batch = false) {
        if (isValidRegionPosition(c, r)) {
            calculateRegion(c, r, batch);
        }
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

    //TODO handles all tileables cases to make sure that two tileable tiles actually want to tile with each other (must add floor tileability functionality)
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

            recalculateRegionsAroundPosition(x, y);

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

            recalculateRegionsAroundPosition(x, y);

            updateTilingForTileClusterAtPosition(x, y);

            updateRenderTilesForTileClusterAtPosition(x, y);
        }
    }

    public void createWallTileAtPosition(int x, int y, string walltype) {
        createWallTileAtPosition(x, y, Tile.nameToTileID(walltype));
    }

    //TODO TESTING PURPOSES ONLY
    public void createTestDoorAtPosition(int x, int y) {
        if (isValidTilePosition(x, y)) {
            uint tile = 0;
            uint tileid = Tile.nameToTileID("testdoor");

            TileData data = Tile.getTileDataForTileID(tileid);

            tile = Tile.setTileId(tile, tileid);
            tile = Tile.setIsTileable(tile, data.isTileable);
            tile = Tile.setIsWall(tile, false);

            tiles[x, y] = tile;

            recalculateRegionsAroundPosition(x, y);

            updateTilingForTileClusterAtPosition(x, y);

            updateRenderTilesForTileClusterAtPosition(x, y);
        }
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
     *
     * Also generates the pawn object container
     */
    private void generateRenderTiles() {
        renderTilesContainer = new GameObject();
        renderTilesContainer.name = "RenderTileContainer";
        renderTilesContainer.transform.parent = this.transform;

        pawnObjectContainer = new GameObject();
        pawnObjectContainer.name = "PawnObjectContainer";
        pawnObjectContainer.transform.parent = this.transform;

        //TODO Create tiles (eventually need to take into account multiple drawing techniques to properly layer tiles, and only apply relevant textures/materials/etc
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                GameObject renderTile = new GameObject();
                SpriteRenderer rend = renderTile.AddComponent<SpriteRenderer>();

                rend.sortingLayerName = "RenderTile";

                renderTile.name = "RenderTile_" + x + "_" + y;

                renderTile.transform.parent = renderTilesContainer.transform;

                renderTile.transform.localPosition = new Vector3(x, y, 0f);

                renderTiles[x, y] = renderTile;

                updateRenderTileAtPosition(x, y);
            }
        }
    }

    /*
     * Adds the pawn to the batch ownership set, which is used to batch calculate region/chunk ownership of pawns, usually after
     * region calculation.
     */
    public void addPawnToBatchOwnershipSet(Pawn pawn) {
        pawnBatchOwnershipSet.Add(pawn);
    }

    /*
     * Calculates the pawn region/chunk ownership for all pawns in the batch ownership set, then clears it
     */
    public void calculatePawnBatchOwnership() {
        foreach (Pawn pawn in pawnBatchOwnershipSet) {
            updatePawnRegionOwnership(pawn);
            updatePawnChunkOwnership(pawn);
        }

        pawnBatchOwnershipSet.Clear();
    }

    /*
     * Determines which chunk the pawn belongs to, if any, and properly updates it, as well as
     * the previous chunk the pawn belonged to, if any.
     */
    public void updatePawnChunkOwnership(Pawn pawn) {
        int x = getPositionX(pawn.getWorldPositionX());
        int y = getPositionY(pawn.getWorldPositionY());

        Chunk currentChunk = pawn.getCurrentChunk();
        Region currentRegion = pawn.getCurrentRegion();

        bool sameChunk = false;
        //Determine new chunk ownership, if it exists
        if (isValidTilePosition(x, y)) {
            //Valid pawn position, do switch operations for chunk ownership
            Region region = getRegionAtPosition(x, y);

            int innerx = getInnerRegionXAtPositionX(x);
            int innery = getInnerRegionYAtPositionY(y);

            Chunk newChunk = region.getChunkAtNodePosition(innerx, innery);

            if (newChunk != null) {
                //We have a valid new chunk for the pawn, check to make sure its not the same as the current
                if (currentChunk == null || !currentChunk.Equals(newChunk)) {
                    //Chunks are not the same, do switch operations for chunk ownership
                    newChunk.addPawnOwnership(pawn);
                }
                else {
                    sameChunk = true;
                }
            }
        }

        //Clean up old chunk ownership, if any exists and it isnt the same chunk
        if (currentChunk != null && !sameChunk) {
            currentChunk.removePawnOwnership(pawn);
        }
    }

    /*
     * Determines which region the pawn belongs to, if any, and properly updates it, as well as
     * the previous region the pawn belonged to, if any.
     */
    public void updatePawnRegionOwnership(Pawn pawn) {
        int x = getPositionX(pawn.getWorldPositionX());
        int y = getPositionY(pawn.getWorldPositionY());

        Region currentRegion = pawn.getCurrentRegion();

        bool sameRegion = false;
        if (isValidTilePosition(x, y)) {
            //Valid pawn position, do switch operations for region ownership
            Region region = getRegionAtPosition(x, y);

            if (currentRegion == null || !currentRegion.Equals(region)) {
                region.addPawnOwnership(pawn);
            }
            else {
                sameRegion = true;
            }
        }

        //Clean up old region ownership, if any exists and it isnt the same region
        if (currentRegion != null && !sameRegion) {
            currentRegion.removePawnOwnership(pawn);
        }
    }

    public uint getTileAtPosition(int x, int y) {
        return tiles[x, y];
    }

    private int getRegionXAtPositionX(int x) {
        return (int) Mathf.Floor(x / (float) regionWidth);
    }

    private int getRegionYAtPositionY(int y) {
        return (int) Mathf.Floor(y / (float) regionHeight);
    }

    public Region getRegionAtPosition(int x, int y) {
        return regions[getRegionXAtPositionX(x), getRegionYAtPositionY(y)];
    }

    public Region getRegionAtMatrixIndex(int column, int row) {
        return regions[column, row];
    }

    public int getInnerRegionXAtPositionX(int x) {
        return x % regionWidth;
    }

    public int getInnerRegionYAtPositionY(int y) {
        return y % regionHeight;
    }

    public int getPositionX(float x) {
        return (int) Mathf.Floor(x);
    }

    public int getPositionY(float y) {
        return (int) Mathf.Floor(y);
    }

    public int getPositionXAtInnerRegionX(Region region, int x) {
        return (region.getWidth() * region.getColumn()) + x;
    }

    public int getPositionYAtInnerRegionY(Region region, int y) {
        return (region.getHeight() * region.getRow()) + y;
    }

    public Vector3 getCenterWorldPositionAtLevelPosition(int x, int y) {
        return new Vector3((float) (x + .5f), (float) (y + .5f), 0f);
    }

    private void initRegions() {
        for (int c = 0; c < regionColumns; c++) {
            for (int r = 0; r < regionRows; r++) {
                regions[c, r] = new Region(this, regionWidth, regionHeight, c, r);
            }
        }
    }

    public GameObject getPawnObjectContainer() {
        return pawnObjectContainer;
    }

    public void load() {
        generateLevel();

        loaded = true;
    }

    public ChunkConnectivity getChunkConnections() {
        return chunkConnections;
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

    public int getRegionColumnCount() {
        return regionColumns;
    }

    public int getRegionRowCount() {
        return regionRows;
    }
}
