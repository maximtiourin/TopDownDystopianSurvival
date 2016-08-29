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
        //calculateChunkConnectivityInner();
        calculateChunkConnectivityOuter();
    }

    //Goes through all outer border chunks and calculates their connectivity
    private void calculateChunkConnectivityOuter() {
        //Make sure to remember to account for edge positions that have already been
        //connect -mirrored by door connectivity, check if a hash exists already for that tile to invalidate it

        //FOUND THE CAUSE OF THE BUG: With mirroring, the chunk purges its mirror connection when regions are being recalculated, undoing the mirror effect!!!!

        //TODO TODO TODO TODO TODO
        //Try to implement general chunk connectivity algorithm, instead of trying to do a special mirroring case with doors, since each chunk
        //has a chance to purge its mirrored connections when they are across neighboring regions
    }

    //Goes through all inner "door" chunks and calculates their connectivity, while mirroring connectivity for their connecting neighbor chunks
    //This will allow calculating connectivity for doors without having to make a more advanced general algorithm for calculating chunk connectivity
    private void calculateChunkConnectivityInner() {
        foreach (Chunk doorChunk in doors) {
            Node door = doorChunk.getNodes()[0];
            int lx = level.getPositionXAtInnerRegionX(this, door.x);
            int ly = level.getPositionYAtInnerRegionY(this, door.y);

            List<uint> connections = doorChunk.getConnectionHashes();
            bool up = !connections.Contains(doorGetQuickTopHash(door)) && isValidDestinationNodeForConnection(lx, ly + 1);
            bool down = !connections.Contains(doorGetQuickBotHash(door)) && isValidDestinationNodeForConnection(lx, ly - 1);
            bool left = !connections.Contains(doorGetQuickLeftHash(door)) && isValidDestinationNodeForConnection(lx - 1, ly);
            bool right = !connections.Contains(doorGetQuickRightHash(door)) && isValidDestinationNodeForConnection(lx + 1, ly);

            //#Note - Since up/down/etc are only valid if the shift position is already valid, we dont need to error check shiftx, shifty
            if (up) {
                createDoorChunkConnection(doorChunk, door, lx, ly, 0, 1, false, ChunkConnectivity.Configuration.Horizontal);
            }
            if (down) {
                createDoorChunkConnection(doorChunk, door, lx, ly, 0, -1, true, ChunkConnectivity.Configuration.Horizontal);
            }
            if (left) {
                createDoorChunkConnection(doorChunk, door, lx, ly, -1, 0, true, ChunkConnectivity.Configuration.Vertical);
            }
            if (right) {
                createDoorChunkConnection(doorChunk, door, lx, ly, 1, 0, false, ChunkConnectivity.Configuration.Vertical);
            }
        }
    }

    //inner is true if the hash should be generated on top of the door node, false otherwise. See calculateChunkConnectivityInner for why things like shiftRegion aren't error checked
    private void createDoorChunkConnection(Chunk doorChunk, Node door, int lx, int ly, int shiftx, int shifty, bool inner, ChunkConnectivity.Configuration config) {
        Region shiftRegion = level.getRegionAtPosition(lx + shiftx, ly + shifty);

        Node shiftNode;
        Chunk shiftChunk;

        bool test = false;
        if (isPositionEqual(shiftRegion)) {
            //Shift Node is within our current region
            shiftNode = nodes[door.x + shiftx, door.y + shifty];
            shiftChunk = fillChunkMap[shiftNode.fillID];
        }
        else {
            test = true;

            //Shift Node is within neighboring region
            int innerx = level.getInnerRegionXAtPositionX(lx + shiftx);
            int innery = level.getInnerRegionYAtPositionY(ly + shifty);
            shiftNode = shiftRegion.getNodeAtNodePosition(innerx, innery);
            shiftChunk = shiftRegion.getChunkAtNodePosition(innerx, innery);
        }

        //Generate ChunkConnectivity hash
        int innershiftx = (inner) ? (0) : (shiftx);
        int innershifty = (inner) ? (0) : (shifty);
        uint hash = ChunkConnectivity.generateConnectivityHash(lx + innershiftx, ly + innershifty, 1, config);

        ChunkConnectivity connections = level.getChunkConnections();
        connections.connectChunk(hash, doorChunk);
        connections.connectChunk(hash, shiftChunk);

        if (test) {
            Debug.Log("Region " + shiftRegion.getColumn() + ", " + shiftRegion.getRow() + " : Node " + shiftNode.x + ", " + shiftNode.y);
            Debug.Log("Chunk " + shiftChunk.fillID + " : Connections Count = " + shiftChunk.getConnectionHashes().Count);
        }
    }

    private bool isValidDestinationNodeForConnection(int x, int y) {
        return level.isValidTilePosition(x, y) && (level.isDoorAtGridPosition(x, y) || !level.isObstacleAtGridPosition(x, y));
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
