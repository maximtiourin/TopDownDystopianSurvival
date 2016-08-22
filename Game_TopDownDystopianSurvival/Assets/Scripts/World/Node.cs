using System.Collections;

/*
 * A node is used in flood fill algorithms to keep track of connectivity status of cells in a region, as well as maintain subset entity lists for that region
 */
public class Node {
    public int fillID;

    public Node() {
        fillID = -1;
    }
}
