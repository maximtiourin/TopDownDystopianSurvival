using System.Collections;

/*
 * A node is used in flood fill algorithms to keep track of connectivity status of cells in a region, as well as maintain subset entity lists for that region
 */
public class Node {
    public static readonly int UNVISITED = -1;
    public static readonly int OBSTACLE = -2;

    public int fillID;

    public int x;
    public int y;

    public Node(int x = 0, int y = 0) {
        fillID = UNVISITED;

        this.x = x;
        this.y = y;
    }
}
