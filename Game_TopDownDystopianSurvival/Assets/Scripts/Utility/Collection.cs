using UnityEngine;
using System.Collections;

/**
 * Utility static methods for dealing with collections that aren't available to Unity's C#
 * @author Maxim Tiourin
 */
public class Collection {
    public static bool Contains<T>(IEnumerable collection, T element) {
        foreach (T e in collection) {
            if (e.Equals(element)) {
                return true;
            }
        }

        return false;
    }
}
