using UnityEngine;
using System.Collections;

public static class Identifier {
    public static long GUID = 0;

    public static long getGlobalUniqueIdentifier() {
        return ++GUID;
    }
}
