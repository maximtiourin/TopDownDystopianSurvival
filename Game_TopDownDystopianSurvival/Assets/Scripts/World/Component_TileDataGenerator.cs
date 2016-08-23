using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/*
 * This component allows creating a rudimentary key value pairing between a name and some relevant tile data, which then gets
 * stored inside of the Tile static hashmap relating a name 
 */
public class Component_TileDataGenerator : MonoBehaviour, Loadable {
    [Serializable]
    public struct Data {
        public string name;
        public Sprite sprite;
        public Shader shader;
        public Color mainColor;
        public bool isNormalMapped;
        public Texture normalMap;
        public bool isShiny;
        public float shininess;
        public Color specularColor;
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

    public void load() {
        if (!Tile.generated) {
            Tile.tileids = new Dictionary<string, ushort>();
            Tile.tileData = new Dictionary<ushort, TileData>();

            //Generate TileData
            foreach (Data data in tileset) {
                ushort tileid = Tile.generateTileID();

                Material material = Script_SpriteRenderer_GenerateMaterial.generateMaterialReference("tiledata_" + tileid, data.shader, data.mainColor,
                    data.isNormalMapped, data.normalMap, data.isShiny, data.shininess, data.specularColor);

                Sprite sprite = data.sprite;

                Tile.tileids.Add(data.name, tileid);
                Tile.tileData.Add(tileid, new TileData(material, sprite));
            }

            Tile.generated = true;
        }

        loaded = true;
    }

    public bool isLoaded() {
        return loaded;
    }
}
