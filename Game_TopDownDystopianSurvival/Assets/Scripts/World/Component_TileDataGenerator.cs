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

    [Serializable]
    public class Data {
        public string name;
        public bool isTileable;
        public Sprite sprite;
        public Shader shader;
        public Color mainColor;
        public bool isNormalMapped;
        public Texture normalMap;
        public bool isShiny;
        public float shininess;
        public Color specularColor;

        public Data() {
            name = BLANK;
            isTileable = false;
            sprite = null;
            shader = null;
            mainColor = new Color(1f, 1f, 1f);
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

    public void addData(Data data) {
        Data[] newts = new Data[tileset.Length + 1];
        Array.Copy(tileset, newts, tileset.Length);
        newts[newts.Length - 1] = data;
        tileset = newts;
    }

    public void copyDataExceptName(Data data) {
        Data ndata = new Data();
        ndata.isTileable = data.isTileable;
        ndata.sprite = data.sprite;
        ndata.shader = data.shader;
        ndata.mainColor = data.mainColor;
        ndata.isNormalMapped = data.isNormalMapped;
        ndata.normalMap = data.normalMap;
        ndata.isShiny = data.isShiny;
        ndata.shininess = data.shininess;
        ndata.specularColor = data.specularColor;

        addData(ndata);
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

            //Generate TileData
            foreach (Data data in tileset) {
                if (!data.name.Equals(BLANK)) {
                    uint tileid = Tile.generateTileID();

                    Material material = Script_SpriteRenderer_GenerateMaterial.generateMaterialReference("tiledata_" + tileid, data.shader, data.mainColor,
                        data.isNormalMapped, data.normalMap, data.isShiny, data.shininess, data.specularColor);

                    Sprite sprite = data.sprite;

                    Tile.tileids.Add(data.name, tileid);
                    Tile.tileData.Add(tileid, new TileData(tileid, data.name, data.isTileable, material, sprite));
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
