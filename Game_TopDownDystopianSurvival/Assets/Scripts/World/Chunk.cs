using System.Collections;
using System.Collections.Generic;

public class Chunk {
    private List<Node> nodes;
    private Dictionary<int, Node> nodeMap;

    private int fillID;

    public Chunk(int fillID) {
        nodes = new List<Node>();
        nodeMap = new Dictionary<int, Node>();

        this.fillID = fillID;
    }

    public void addNode(int key, Node value) {
        nodes.Add(value);
        nodeMap.Add(key, value);
    }
}
