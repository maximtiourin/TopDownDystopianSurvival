using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Region {
    private Level level;

    private int width; //The number of nodes in the region in the x axis
    private int height; //The number of nodes in the region in the y axis

    private int column; //The region's column in the overall level
    private int row; //The region's row in the overall level

    private Node[,] nodes;

    private List<Chunk> chunks; //List of all chunks within the region
    private List<Chunk> doors; // List of all "door" chunks within the region
    private Dictionary<int, Chunk> fillChunkMap; //Hashmap mapping fillIds to Chunks within the region
    private int fillID; //A unique identifier denoting a subset of connected nodes within the region (which get represented by a chunk)

    public Region(Level level, int width, int height, int column, int row) {
        this.level = level;

        this.width = width;
        this.height = height;

        this.column = column;
        this.row = row;

        nodes = new Node[this.width, this.height];
        initNodes();

        initChunks();
        resetFillID();
    }

    private void initNodes() {
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                nodes[x, y] = new Node(x, y);
            }
        }
    }

    private void initChunks() {
        chunks = new List<Chunk>();
        doors = new List<Chunk>();
        fillChunkMap = new Dictionary<int, Chunk>();
    }

    public void calculateRegion() {
        //Remove this region's chunks from the connectivity map
        foreach (Chunk chunk in chunks) {
            List<uint> connections = chunk.getConnectionHashes();

            int index = 0;
            while (connections != null && index < connections.Count) {
                uint hash = connections[index];
                level.getChunkConnections().disconnectChunk(hash, chunk);
                index++;
            }
        }

        //Clean up all region data in preparation for new calculations
        initNodes();
        chunks.Clear();
        doors.Clear();
        fillChunkMap.Clear();
        resetFillID();

        //Calculate Chunks
        for (int x = 0; x < width; x++) {
            for (int y = 0; y < height; y++) {
                BFSNodes(x, y);
            }
        }

        //Add this region's chunks to the connectivity map
        calculateChunkConnectivityEdge(0, width - 1, height - 1, height - 1, 0, 1, 0, 1, ChunkConnectivity.Configuration.Horizontal);   //Top
        calculateChunkConnectivityEdge(0, width - 1, 0, 0, 0, -1, 0, 0, ChunkConnectivity.Configuration.Horizontal);                    //Bot
        calculateChunkConnectivityEdge(0, 0, 0, height - 1, -1, 0, 0, 0, ChunkConnectivity.Configuration.Vertical);                     //Left
        calculateChunkConnectivityEdge(width - 1, width - 1, 0, height - 1, 1, 0, 1, 0, ChunkConnectivity.Configuration.Vertical);      //Right
        calculateChunkConnectivityDoors();
        calculateChunkConnectivityInnerToDoors();
    }

    //Goes through all appropriate nodes and generates chunk connectivity information for the appropriate chunks.
    //A region is responsible for fully building all correct connectivity information for its own chunks
    private void calculateChunkConnectivityEdge(int xmin, int xmax, int ymin, int ymax, int destXOff, int destYOff, int connectXOff, int connectYOff, ChunkConnectivity.Configuration config) {
        // ***** Loop through edge nodes to build edge connectivity (account for any doors that might connect to edge nodes)
        bool chain = false;
        int chainlen = 0;
        Node chainStartNode = null;
        Chunk chainStartChunk = null;
        for (int x = xmin; x <= xmax; x++) {
            for (int y = ymin; y <= ymax; y++) {

                int lx = level.getPositionXAtInnerRegionX(this, x);
                int ly = level.getPositionYAtInnerRegionY(this, y);

                int lxoff = lx + destXOff;
                int lyoff = ly + destYOff;

                Region offsetDestRegion = null;
                Node dest = null;

                bool validOffset = level.isValidTilePosition(lxoff, lyoff);

                if (validOffset) {
                    offsetDestRegion = level.getRegionAtPosition(lxoff, lyoff);
                    int offinnerx = level.getInnerRegionXAtPositionX(lxoff);
                    int offinnery = level.getInnerRegionYAtPositionY(lyoff);

                    dest = offsetDestRegion.getNodeAtNodePosition(offinnerx, offinnery);
                }

                Node node = nodes[x, y];

                Chunk chunk = getChunkAtNodePosition(node.x, node.y);

                if (isNodeObstacleOrDoor(node)) {
                    //Conclude a regular chain
                    if (chain) {
                        //Conclude and generate connection for chain
                        int cslx = level.getPositionXAtInnerRegionX(this, chainStartNode.x);
                        int csly = level.getPositionYAtInnerRegionY(this, chainStartNode.y);

                        level.getChunkConnections().connectChunk(ChunkConnectivity.generateConnectivityHash(cslx + connectXOff, csly + connectYOff, chainlen, config), chainStartChunk);

                        chain = false;
                        chainlen = 0;
                        chainStartNode = null;
                        chainStartChunk = null;
                    }
                }
                else {
                    if (chain) {
                        //Continue Chain
                        if (validOffset && !offsetDestRegion.isNodeObstacleOrDoor(dest)) {
                            chainlen++;
                        }
                        else {
                            //Conclude and generate connection for chain
                            int cslx = level.getPositionXAtInnerRegionX(this, chainStartNode.x);
                            int csly = level.getPositionYAtInnerRegionY(this, chainStartNode.y);

                            level.getChunkConnections().connectChunk(ChunkConnectivity.generateConnectivityHash(cslx + connectXOff, csly + connectYOff, chainlen, config), chainStartChunk);

                            chain = false;
                            chainlen = 0;
                            chainStartNode = null;
                            chainStartChunk = null;
                        }
                    }
                    else {
                        //Start chain
                        if (validOffset && !offsetDestRegion.isNodeObstacleOrDoor(dest)) {
                            //Valid chain start
                            chainlen = 1;
                            chainStartNode = node;
                            chainStartChunk = chunk;
                            chain = true;
                        }
                    }

                    //Check for doors in 4 cardinal directions and generate connections to them, avoids duplicates
                    calculateAdjacentDoorConnectivityForChunkAtNode(chunk, node);
                }
            }
        }

        //Conclude a tail-end chain
        if (chain) {
            //Conclude and generate connection for chain
            int cslx = level.getPositionXAtInnerRegionX(this, chainStartNode.x);
            int csly = level.getPositionYAtInnerRegionY(this, chainStartNode.y);

            level.getChunkConnections().connectChunk(ChunkConnectivity.generateConnectivityHash(cslx + connectXOff, csly + connectYOff, chainlen, config), chainStartChunk);

            chain = false;
            chainlen = 0;
            chainStartNode = null;
            chainStartChunk = null;
        }
    }

    //Checks all door chunks inside of the region and their edges for for connectivity. Generates connections accordingly.
    private void calculateChunkConnectivityDoors() {
        int index = 0;
        while (doors != null && index < doors.Count) {
            Chunk chunk = doors[index];
            Node node = chunk.getNodes()[0];

            calculateAdjacentConnectivityForDoorAtNode(chunk, node);

            index++;
        }
    }

    //Checks for door connectivity of all inner nodes. An inner node is one that does not touch the edge of the region. Generates connections accordingly.
    private void calculateChunkConnectivityInnerToDoors() {
        int xmin = 1;
        int xmax = width - 2;
        int ymin = 1;
        int ymax = height - 2;


        for (int x = xmin; x <= xmax; x++) {
            for (int y = ymin; y <= ymax; y++) {
                Node node = nodes[x, y];
                Chunk chunk = getChunkAtNodePosition(node.x, node.y);

                if (!isNodeObstacleOrDoor(node)) {
                    calculateAdjacentDoorConnectivityForChunkAtNode(chunk, node);
                }            
            }
        }
    }

    //Will Avoid creating duplicate connections, used for Chunk to Door connections
    private void calculateAdjacentDoorConnectivityForChunkAtNode(Chunk chunk, Node node) {
        int lx = level.getPositionXAtInnerRegionX(this, node.x);
        int ly = level.getPositionYAtInnerRegionY(this, node.y);

        List<uint> connections = chunk.getConnectionHashes();

        uint doorTopHash = doorGetQuickTopHash(node);
        uint doorBotHash = doorGetQuickBotHash(node);
        uint doorLeftHash = doorGetQuickLeftHash(node);
        uint doorRightHash = doorGetQuickRightHash(node);

        bool checkTop = !connections.Contains(doorTopHash) && isValidDestinationDoorNodeForConnection(lx, ly + 1);
        bool checkBot = !connections.Contains(doorBotHash) && isValidDestinationDoorNodeForConnection(lx, ly - 1);
        bool checkLeft = !connections.Contains(doorLeftHash) && isValidDestinationDoorNodeForConnection(lx - 1, ly);
        bool checkRight = !connections.Contains(doorRightHash) && isValidDestinationDoorNodeForConnection(lx + 1, ly);

        if (checkTop) {
            level.getChunkConnections().connectChunk(doorTopHash, chunk);
        }
        if (checkBot) {
            level.getChunkConnections().connectChunk(doorBotHash, chunk);
        }
        if (checkLeft) {
            level.getChunkConnections().connectChunk(doorLeftHash, chunk);
        }
        if (checkRight) {
            level.getChunkConnections().connectChunk(doorRightHash, chunk);
        }
    }

    //Will Avoid creating duplicate connections, used for Door to Chunk connections
    private void calculateAdjacentConnectivityForDoorAtNode(Chunk chunk, Node node) {
        int lx = level.getPositionXAtInnerRegionX(this, node.x);
        int ly = level.getPositionYAtInnerRegionY(this, node.y);

        List<uint> connections = chunk.getConnectionHashes();

        uint doorTopHash = doorGetQuickTopHash(node);
        uint doorBotHash = doorGetQuickBotHash(node);
        uint doorLeftHash = doorGetQuickLeftHash(node);
        uint doorRightHash = doorGetQuickRightHash(node);

        bool checkTop = !connections.Contains(doorTopHash) && isValidDestinationNodeForConnection(lx, ly + 1);
        bool checkBot = !connections.Contains(doorBotHash) && isValidDestinationNodeForConnection(lx, ly - 1);
        bool checkLeft = !connections.Contains(doorLeftHash) && isValidDestinationNodeForConnection(lx - 1, ly);
        bool checkRight = !connections.Contains(doorRightHash) && isValidDestinationNodeForConnection(lx + 1, ly);

        if (checkTop) {
            level.getChunkConnections().connectChunk(doorTopHash, chunk);
        }
        if (checkBot) {
            level.getChunkConnections().connectChunk(doorBotHash, chunk);
        }
        if (checkLeft) {
            level.getChunkConnections().connectChunk(doorLeftHash, chunk);
        }
        if (checkRight) {
            level.getChunkConnections().connectChunk(doorRightHash, chunk);
        }
    }

    private bool isValidDestinationNodeForConnection(int lx, int ly) {
        return level.isValidTilePosition(lx, ly) && (level.isDoorAtGridPosition(lx, ly) || !level.isObstacleAtGridPosition(lx, ly));
    }

    private bool isValidDestinationDoorNodeForConnection(int lx, int ly) {
        return level.isValidTilePosition(lx, ly) && (level.isDoorAtGridPosition(lx, ly));
    }

    public Node getNodeAtNodePosition(int x, int y) {
        if (isValidNodeIndex(x, y)) {
            return nodes[x, y];
        }
        else {
            return null;
        }
    }

    public bool isChunkAtNodePosition(int x, int y) {
        if (isValidNodeIndex(x, y)) {
            Node node = nodes[x, y];

            return fillChunkMap.ContainsKey(node.fillID);
        }
        else {
            return false;
        }
    }

    public Chunk getChunkAtNodePosition(int x, int y) {
        if (isChunkAtNodePosition(x, y)) {
            return fillChunkMap[nodes[x, y].fillID];
        }
        else {
            return null;
        }
    }

    //Breadth first search nodes for pathability and assign unique fillid, also assign nodes to a chunk
    private void BFSNodes(int x, int y) {
        if (isValidNodeIndex(x, y)) {
            Node root = nodes[x, y];

            if (isValidDoorNode(root)) {
                Node node = root;

                int fill = generateFillID();

                Chunk chunk = new Chunk(fill);

                node.fillID = fill;

                chunk.addNode(getMappedNodePositionToIndex(node), node);

                //Add door chunk to region
                chunks.Add(chunk);
                doors.Add(chunk);
                fillChunkMap[fill] = chunk;
            }
            else if (isValidNode(root)) {
                Queue<Node> queue = new Queue<Node>();

                int fill = generateFillID();
                
                Chunk chunk = new Chunk(fill); //Generate Chunk to populate with nodes

                queue.Enqueue(root);

                int bugBreak = width * height + 1; //DEBUG
                int loopCount = 0; //DEBUG
                while (queue.Count > 0 && loopCount < bugBreak) { //DEBUG
                    Node node = queue.Dequeue();

                    node.fillID = fill;

                    chunk.addNode(getMappedNodePositionToIndex(node), node); //Add node to chunk

                    //Get valid neighboring nodes
                    for (int dx = -1; dx <= 1; dx++) {
                        for (int dy = -1; dy <= 1; dy++) {
                            if (!((dx == 0) && (dy == 0)) && (Math.Abs(dx) != Math.Abs(dy))) {
                                int nx = node.x + dx;
                                int ny = node.y + dy;

                                if (isValidNodeIndex(nx, ny)) {
                                    Node neighbor = nodes[nx, ny];

                                    if (isValidNode(neighbor) && !isValidDoorNode(neighbor)) {
                                        if (!queue.Contains(neighbor)) {
                                            queue.Enqueue(neighbor);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    loopCount++; //DEBUG
                }

                if (loopCount == bugBreak) Debug.Log("Infinite Loop Stopped Region[" + column + ", " + row + "] BFSNodes(" + x + ", " + y + ")"); //DEBUG

                //Add chunk to region
                chunks.Add(chunk);
                fillChunkMap[fill] = chunk;
            }
        }
    }

    private bool isValidNode(Node node) {
        return isNodeUnvisited(node) && !isNodeObstacle(node);
    }

    private bool isValidDoorNode(Node node) {
        return isNodeUnvisited(node) && isNodeDoor(node);
    }

    private bool isNodeUnvisited(Node node) {
        return node.fillID == Node.UNVISITED;
    }

    private bool isNodeObstacle(Node node) {
        if (node.fillID == Node.OBSTACLE) {
            return true;
        }
        else {
            if (level.isObstacleAtRegionPosition(this, node.x, node.y)) {
                node.fillID = Node.OBSTACLE;
                return true;
            }
            else {
                return false;
            }
        }
    }

    private bool isNodeDoor(Node node) {
        return level.isDoorAtRegionPosition(this, node.x, node.y);
    }

    private bool isNodeObstacleOrDoor(Node node) {
        return isNodeObstacle(node) || isNodeDoor(node);
    }

    public bool isValidNodeIndex(int x, int y) {
        return (x >= 0 && x < width) && (y >= 0 && y < height);
    }

    public bool isValidRegionToLevelIndex(int x, int y) {
        return level.isValidRegionInnerPosition(this, x, y);
    }

    public int generateFillID() {
        return fillID++;
    }

    /*
     * Maps the x and y position of a node to a single integer for use as a unique key
     */
    public int getMappedNodePositionToIndex(int x, int y) {
        return x * height + y;
    }

    /*
     * Maps the x and y position of a node to a single integer for use as a unique key
     */
    public int getMappedNodePositionToIndex(Node node) {
        return getMappedNodePositionToIndex(node.x, node.y);
    }

    public int getRegionHashCode() {
        return column * level.getRegionRowCount() + row;
    }

    public int getWidth() {
        return width;
    }

    public int getHeight() {
        return height;
    }

    public int getColumn() {
        return column;
    }

    public int getRow() {
        return row;
    }

    private uint doorGetQuickTopHash(Node door) {
        int lx = level.getPositionXAtInnerRegionX(this, door.x);
        int ly = level.getPositionYAtInnerRegionY(this, door.y);
        return ChunkConnectivity.generateConnectivityHash(lx, ly + 1, 1, ChunkConnectivity.Configuration.Horizontal);
    }

    private uint doorGetQuickBotHash(Node door) {
        int lx = level.getPositionXAtInnerRegionX(this, door.x);
        int ly = level.getPositionYAtInnerRegionY(this, door.y);
        return ChunkConnectivity.generateConnectivityHash(lx, ly, 1, ChunkConnectivity.Configuration.Horizontal);
    }

    private uint doorGetQuickLeftHash(Node door) {
        int lx = level.getPositionXAtInnerRegionX(this, door.x);
        int ly = level.getPositionYAtInnerRegionY(this, door.y);
        return ChunkConnectivity.generateConnectivityHash(lx, ly, 1, ChunkConnectivity.Configuration.Vertical);
    }

    private uint doorGetQuickRightHash(Node door) {
        int lx = level.getPositionXAtInnerRegionX(this, door.x);
        int ly = level.getPositionYAtInnerRegionY(this, door.y);
        return ChunkConnectivity.generateConnectivityHash(lx + 1, ly, 1, ChunkConnectivity.Configuration.Vertical);
    }

    public Chunk getChunk(int fillID) {
        if (fillChunkMap.ContainsKey(fillID)) {
            return fillChunkMap[fillID];
        }
        else {
            return null;
        }
    }

    public bool isPositionEqual(Region other) {
        return (row == other.getRow()) && (column == other.getColumn());
    }

    private void resetFillID() {
        fillID = 0;
    }
}
