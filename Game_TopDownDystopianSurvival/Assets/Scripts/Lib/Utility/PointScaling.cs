using System;
using ClipperLib;
using Poly2Tri;
using UnityEngine;

/**
 * Utility class for converting between the various data types used to represent points between different polygon libraries and Unity
 * @author - Maxim Tiourin
 */
public class PointScaling{
    public const long scalar = 10000000; //Value by which to scale integers to represent their floating point counterparts, aiming to preserve 7 decimal points of precision (Unity's float precision)

    public static Poly2Tri.Point2D clipperToPoly2tri(ClipperLib.IntPoint pt) {
        return new Poly2Tri.Point2D(pt.X / (double) scalar, pt.Y / (double) scalar);
    }

    public static Vector2 clipperToUnity(ClipperLib.IntPoint pt) {
        return new Vector2(pt.X / (float) scalar, pt.Y / (float) scalar);
    }

    public static ClipperLib.IntPoint poly2triToClipper(Poly2Tri.Point2D pt) {
        return new ClipperLib.IntPoint((long) (pt.X * scalar), (long) (pt.Y * scalar));
    }

    public static Vector2 poly2triToUnity(Poly2Tri.Point2D pt) {
        return new Vector2(pt.Xf, pt.Yf);
    }

    public static ClipperLib.IntPoint unityToClipper(Vector2 pt) {
        return new ClipperLib.IntPoint((long) (pt.x * scalar), (long) (pt.y * scalar));
    }

    public static Poly2Tri.Point2D unityToPoly2tri(Vector2 pt) {
        return new Poly2Tri.Point2D((double) pt.x, (double) pt.y);
    }
}
