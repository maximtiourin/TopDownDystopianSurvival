using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * This component allows creating a rudimentary key value pairing between a name and some relevant tile data, which then gets
 * stored inside of the Tile static hashmap relating a name 
 */
public class Component_TileDataGenerator : MonoBehaviour, Loadable {
    public static string BLANK = "";
    public static int TILEABLE_WIDTH = 4;
    public static int TILEABLE_HEIGHT = 4;
    public static int TILEABLE_PIXELS = 64;

    [Serializable]
    public class Data {
        public string name;
        public bool isTileable;
        public Sprite sprite;
        public Texture2D sprites;
        public Shader shader;
        public Color mainColor;
        public Color tileableColor;
        public bool isNormalMapped;
        public Texture normalMap;
        public bool isShiny;
        public float shininess;
        public Color specularColor;

        public Data() {
            name = BLANK;
            isTileable = false;
            sprite = null;
            sprites = null;
            shader = null;
            mainColor = new Color(1f, 1f, 1f);
            tileableColor = Color.black;
            isNormalMapped = false;
            normalMap = null;
            isShiny = false;
            shininess = .016f;
            specularColor = new Color(112f / 255f, 112f / 255f, 112f / 255f);
        }
    }

    public Data[] tileset;

    private bool loaded;

	// Use this for initialization
	void Start () {
        loaded = false;
	}
	
	// Update is called once per frame
	void Update () {

	}

    public void volatileCopy(Data src, Data dest) {
        dest.name = src.name;
        dest.isTileable = src.isTileable;
        dest.sprite = src.sprite;
        dest.sprites = src.sprites;
        dest.shader = src.shader;
        dest.mainColor = src.mainColor;
        dest.tileableColor = src.tileableColor;
        dest.isNormalMapped = src.isNormalMapped;
        dest.normalMap = src.normalMap;
        dest.isShiny = src.isShiny;
        dest.shininess = src.shininess;
        dest.specularColor = src.specularColor;
    }

    public Data shallowCopy(Data data) {
        Data ndata = new Data();
        volatileCopy(data, ndata);
        return ndata;
    }

    public void addData(Data data) {
        Data[] newts = new Data[tileset.Length + 1];
        Array.Copy(tileset, newts, tileset.Length);
        newts[newts.Length - 1] = data;
        tileset = newts;
    }

    public void copyData(Data data) {
        Data ndata = shallowCopy(data);

        addData(ndata);
    }

    public void copyDataExceptName(Data data) {
        Data ndata = shallowCopy(data);
        ndata.name = BLANK;

        addData(ndata);
    }

    public void swapData(Data a, Data b) {
        Data ndata = shallowCopy(a);
        volatileCopy(b, a);
        volatileCopy(ndata, b);
    }

    public void deleteData(int i) {
        int length = tileset.Length;

        if (i == 0) {
            Data[] newts = new Data[length - 1];
            Array.Copy(tileset, i + 1, newts, 0, length - 1);
            tileset = newts;
        }
        else if (i == tileset.Length - 1) {
            Data[] newts = new Data[length - 1];
            Array.Copy(tileset, 0, newts, 0, length - 1);
            tileset = newts;
        }
        else {
            Data[] newts = new Data[length - 1];
            Array.Copy(tileset, 0, newts, 0, i);
            Array.Copy(tileset, i + 1, newts, i, length - 1 - i);
            tileset = newts;
        }
    }

    public void load() {
        if (!Tile.generated) {
            Tile.tileids = new Dictionary<string, uint>();
            Tile.tileData = new Dictionary<uint, TileData>();
            Tile.dynamicFillSprites = new Dictionary<uint, Dictionary<uint, Sprite>>();

            //Generate TileData
            foreach (Data data in tileset) {
                if (!data.name.Equals(BLANK)) {
                    uint tileid = Tile.generateTileID();

                    Material material = Script_SpriteRenderer_GenerateMaterial.generateMaterialReference("tiledata_" + tileid, data.shader, data.mainColor,
                        data.isNormalMapped, data.normalMap, data.isShiny, data.shininess, data.specularColor);

                    Sprite sprite = data.sprite;

                    Sprite[] sprites = null;

                    Color fillColor = Color.black;
                    if (data.isTileable) {
                        sprites = new Sprite[(TILEABLE_WIDTH * TILEABLE_HEIGHT) + 1];
                        for (int x = 0; x < TILEABLE_WIDTH; x++) {
                            for (int y = 0; y < TILEABLE_HEIGHT; y++) {
                                Sprite spr = Sprite.Create(data.sprites, new Rect(x * TILEABLE_PIXELS, y * TILEABLE_PIXELS, TILEABLE_PIXELS, TILEABLE_PIXELS), new Vector2(0, 0), TILEABLE_PIXELS);
                                sprites[y * TILEABLE_WIDTH + x] = spr;
                            }
                        }

                        //Create Fill Sprite
                        fillColor = sprites[0].texture.GetPixel(TILEABLE_PIXELS / 2, TILEABLE_PIXELS - TILEABLE_PIXELS / 4);

                        /*Texture2D tex = new Texture2D(TILEABLE_PIXELS, TILEABLE_PIXELS);
                        tex.filterMode = FilterMode.Point;
                        for (int x = 0; x < TILEABLE_PIXELS; x++) {
                            for (int y = 0; y < TILEABLE_PIXELS; y++) {
                                tex.SetPixel(x, y, fillColor);
                            }
                        }
                        tex.Apply();

                        Sprite fillSprite = Sprite.Create(tex, new Rect(0, 0, TILEABLE_PIXELS, TILEABLE_PIXELS), new Vector2(0, 0), TILEABLE_PIXELS);
                        sprites[TILEABLE_WIDTH * TILEABLE_HEIGHT] = fillSprite; */
                    }

                    Tile.tileids.Add(data.name, tileid);
                    Tile.tileData.Add(tileid, new TileData(tileid, data.name, data.isTileable, material, sprite, sprites, fillColor));
                }
            }

            Tile.generated = true;
        }

        loaded = true;
    }

    public bool isLoaded() {
        return loaded;
    }
}
