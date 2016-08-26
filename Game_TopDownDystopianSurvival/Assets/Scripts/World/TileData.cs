using UnityEngine;
using System.Collections;

/*
 * Structure that holds relevant tile data to aid in storing as a value inside of a hashmap
 */
public class TileData {
    public uint tileid;
    public string name;
    public bool isTileable;
    public Material material;
    public Sprite sprite;
    public Sprite[] sprites;
    public Color fillColor;

    public TileData(uint tileid, string name, bool isTileable, Material material, Sprite sprite, Sprite[] sprites, Color fillColor) {
        this.tileid = tileid;
        this.name = name;
        this.isTileable = isTileable;
        this.material = material;
        this.sprite = sprite;
        this.sprites = sprites;
        this.fillColor = fillColor;
    }
}
