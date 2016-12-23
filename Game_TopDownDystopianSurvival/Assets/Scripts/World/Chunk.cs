using System.Collections;
using System.Collections.Generic;

public class Chunk {
    private List<Node> nodes;
    private Dictionary<int, Node> nodeMap;
    private List<uint> connectionHashes;

    private List<Pawn> pawns; //List of all pawns this chunk has current ownership over

    public int fillID;

    public Chunk(int fillID) {
        nodes = new List<Node>();
        nodeMap = new Dictionary<int, Node>();
        connectionHashes = new List<uint>();

        pawns = new List<Pawn>();

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

    public bool addConnection(uint hash) {
        if (!connectionHashes.Contains(hash)) {
            connectionHashes.Add(hash);
            return true;
        }
        else {
            return false;
        }
    }

    public bool removeConnection(uint hash) {
        return connectionHashes.Remove(hash);
    }

    public List<uint> getConnectionHashes() {
        return connectionHashes;
    }

    public void addPawnOwnership(Pawn pawn) {
        pawns.Add(pawn);
        pawn.setCurrentChunk(this);
    }

    public void removePawnOwnership(Pawn pawn) {
        pawns.Remove(pawn);
        pawn.setCurrentChunk(null);
    }

    public List<Pawn> getPawns() {
        return pawns;
    }

    public void disownPawns() {
        foreach (Pawn pawn in pawns) {
            pawn.setCurrentChunk(null);
        }

        pawns.Clear();
    }
}
