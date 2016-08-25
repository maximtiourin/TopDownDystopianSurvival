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
        tile = Tile.setTileId(tile, test01.tileid);
        tile = Tile.setIsTileable(tile, test01.isTileable);
        tiles[0, 0] = tile;
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

                TileData data = Tile.getTileDataForTileID(tileid);

                rend.sprite = data.sprite;
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
