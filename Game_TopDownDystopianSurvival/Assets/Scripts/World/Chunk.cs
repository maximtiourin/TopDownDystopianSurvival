﻿using System.Collections;
using System.Collections.Generic;

public class Chunk {
    private List<Node> nodes;
    private Dictionary<int, Node> nodeMap;
    private List<uint> connectionHashes;

    public int fillID;

    public Chunk(int fillID) {
        nodes = new List<Node>();
        nodeMap = new Dictionary<int, Node>();
        connectionHashes = new List<uint>();

        this.fillID = fillID;
    }

    public void addNode(int key, Node value) {
        nodes.Add(value);
        nodeMap[key] = value;
    }

    public List<Node> getNodes() {
        return nodes;
    }

    public bool containsNode(int key) {
        return nodeMap.ContainsKey(key);
    }

    public List<uint> getConnectionHashes() {
        return connectionHashes;
    }
}
