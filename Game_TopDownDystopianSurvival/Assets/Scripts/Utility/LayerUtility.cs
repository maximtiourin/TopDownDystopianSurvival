using UnityEngine;
using System.Collections;

/**
 * Has useful utility functions for dealing with layers and their masks.
 * @author - Maxim Tiourin
 */
public class LayerUtility : MonoBehaviour {
    public static int ConstructLayerMask(int[] layers) {
        int layermask = 0;
        foreach (int layer in layers) {
            layermask = 1 << layer;
        }

        return layermask;
    }
}
