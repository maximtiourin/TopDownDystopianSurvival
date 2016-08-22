using System.Collections;
using System.Collections.Generic;

public class Region {
    private int width;
    private int height;

    private Node[,] nodes;

    private List<Chunk> chunks; //List of all chunks within the region

    private Dictionary<int, Chunk> fillChunkMap; //Hashmap mapping fillIds to Chunks within the region

    private int fillID; //A unique identifier denoting a subset of connected nodes within the region (which get represented by a chunk)

    public Region(int width, int height) {
        this.width = width;
        this.height = height;

        nodes = new Node[this.width, this.height];
        initNodes();

        chunks = new List<Chunk>();

        fillChunkMap = new Dictionary<int, Chunk>();

        fillID = 0;
    }

    public int generateFillID() {
        return fillID++;
    }

    private void initNodes() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                nodes[x, y] = new Node();
            }
        }
    }
}
