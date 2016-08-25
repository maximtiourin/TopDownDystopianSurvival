using UnityEngine;
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

    //TODO Temp testing of tileability calculation, eventually can be used to efficiently do the first pass calculation of tileability
    // When modifying tileability during normal runtime, it should be done only for tiles which have been modified.
    private void calculateTiling() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                uint tile = tiles[x, y];

                if (Tile.getIsWall(tile)) {
                    if (x + 1 < width) {
                        uint cmp = tiles[x + 1, y];

                        if (Tile.getIsWall(cmp)) {
                            uint tilebit = Tile.getTileBitwise(tile);
                            uint cmpbit = Tile.getTileBitwise(cmp);

                            tilebit = tilebit | 8;
                            cmpbit = cmpbit | 2;

                            tile = Tile.setTileBitwise(tile, tilebit);
                            cmp = Tile.setTileBitwise(cmp, cmpbit);

                            tiles[x, y] = tile;
                            tiles[x + 1, y] = cmp;
                        }
                    }
                    if (y + 1 < width) {
                        uint cmp = tiles[x, y + 1];

                        if (Tile.getIsWall(cmp)) {
                            uint tilebit = Tile.getTileBitwise(tile);
                            uint cmpbit = Tile.getTileBitwise(cmp);

                            tilebit = tilebit | 1;
                            cmpbit = cmpbit | 4;

                            tile = Tile.setTileBitwise(tile, tilebit);
                            cmp = Tile.setTileBitwise(cmp, cmpbit);

                            tiles[x, y] = tile;
                            tiles[x, y + 1] = cmp;
                        }
                    }
                }
            }
        }
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

                uint tile = tiles[x, y];

                uint tileid = Tile.getTileId(tile);
                uint bitwise = Tile.getTileBitwise(tile);

                TileData data = Tile.getTileDataForTileID(tileid);

                if (data.isTileable) {
                    rend.sprite = data.sprites[bitwise];
                }
                else {
                    rend.sprite = data.sprite;
                }

                rend.sharedMaterial = data.material;

                renderTile.name = "RenderTile_" + x + "_" + y;

                renderTile.transform.parent = renderTilesContainer.transform;

                renderTile.transform.localPosition = new Vector3(x, y, 0f);
            }
        }
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
