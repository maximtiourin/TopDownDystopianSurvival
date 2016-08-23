using System.Collections;
using System.Collections.Generic;

public class Region {
    private Level level;

    private int width; //The number of nodes in the region in the x axis
    private int height; //The number of nodes in the region in the y axis

    private int column; //The region's column in the overall level
    private int row; //The region's row in the overall level

    private Node[,] nodes;

    private List<Chunk> chunks; //List of all chunks within the region
    private Dictionary<int, Chunk> fillChunkMap; //Hashmap mapping fillIds to Chunks within the region
    private int fillID; //A unique identifier denoting a subset of connected nodes within the region (which get represented by a chunk)

    public Region(Level level, int width, int height, int column, int row) {
        this.level = level;

        this.width = width;
        this.height = height;

        this.column = column;
        this.row = row;

        nodes = new Node[this.width, this.height];

        chunks = new List<Chunk>();
        fillChunkMap = new Dictionary<int, Chunk>();
        resetFillID();
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

    private void resetFillID() {
        fillID = 0;
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
    }

    private void calculateRegion() {
        //First clean up all region data in preparation for new calculations
        initNodes();
        chunks.Clear();
        fillChunkMap.Clear();
        resetFillID();
    }

    //Breadth first search nodes for pathability and assign unique fillid, also assign nodes to a chunk
    private void BFSNodes(int x, int y) {
        if (isValidNodeIndex(x, y)) {
            Node root = nodes[x, y];

            if (isValidBFSNode(root)) {
                Queue<Node> queue = new Queue<Node>();

                int fill = generateFillID();
                
                Chunk chunk = new Chunk(fill); //Generate Chunk to populate with nodes

                queue.Enqueue(root);

                while (queue.Count > 0) {
                    Node node = queue.Dequeue();

                    node.fillID = fill;

                    chunk.addNode(getMappedNodePositionToIndex(node), node); //Add node to chunk

                    //Get valid neighboring nodes
                    for (int dx = -1; dx <= 1; dx++) {
                        for (int dy = -1; dy <= 1; dy++) {
                            if (!((dx == 0) && (dy == 0))) {
                                int nx = x + dx;
                                int ny = y + dy;

                                if (isValidNodeIndex(nx, ny)) {
                                    Node neighbor = nodes[nx, ny];

                                    if (isValidBFSNode(neighbor)) {
                                        queue.Enqueue(neighbor);
                                    }
                                }
                            }
                        }
                    }
                }

                //Add chunk to region
                chunks.Add(chunk);
                fillChunkMap.Add(fill, chunk);
            }
        }
    }

    private bool isValidBFSNode(Node node) {
        return node.fillID != Node.OBSTACLE && !level.isObstacleAtRegionPosition(this, node.x, node.y);
    }

    private bool isValidNodeIndex(int x, int y) {
        return (x >= 0 && x < width) && (y >= 0 && y < height);
    }
}
