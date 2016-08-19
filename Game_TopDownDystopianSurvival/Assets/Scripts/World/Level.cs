using UnityEngine;
using System.Collections;

public class Level : MonoBehaviour {
    public int width = 200;
    public int height = 200;
    public int depth = 100;
    protected ushort[,,] tiles;

	// Use this for initialization
	void Start () {
        tiles = new ushort[width, height, depth];
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
