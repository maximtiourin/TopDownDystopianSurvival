using UnityEngine;
using System.Collections;

/*
 * Structure that holds relevant tile data to aid in storing as a value inside of a hashmap
 */
public class TileData {
    public Material material;
    public Sprite sprite;

    public TileData(Material material, Sprite sprite) {
        this.material = material;
        this.sprite = sprite;
    }
}
