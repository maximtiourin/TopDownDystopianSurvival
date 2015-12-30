using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace FizzikLib {
    /**
     * Simple Polygon class that contains a list of vertices of a simple polygon as Vector2 types, with
     * the polygon defined in clockwise order. Also contains various static methods for the creation of a
     * Simple Polygon given various other objects.
     * @author - Maxim Tiourin
     */
    public class SimplePolygon {
        private List<Vector2> vertices;

        public SimplePolygon() {
            vertices = new List<Vector2>();
        }

        public void addVertex(Vector2 v) {
            vertices.Add(v);
        }

        public Vector2 getVertex(int index) {
            return vertices[index];
        }

        public List<Vector2> getVertices() {
            return vertices;
        }

        /**
         * Returns a SimplePolygon containing the vertexes of a BoxCollider2D in world space,
         * taking into account all aspects of its transform.
         *
         * Vertex indexing starts at 0 in the bottom left corner, and moves in a clockwise direction
         */
        public static SimplePolygon boxCollider2DToSimplePolygon(BoxCollider2D collider) {
            float top = collider.offset.y + (collider.size.y / 2f);
            float bottom = collider.offset.y - (collider.size.y / 2f);
            float left = collider.offset.x - (collider.size.x / 2f);
            float right = collider.offset.x + (collider.size.x / 2f);

            Vector3 topLeft = collider.transform.TransformPoint(new Vector3(left, top, 0f));
            Vector3 topRight = collider.transform.TransformPoint(new Vector3(right, top, 0f));
            Vector3 bottomLeft = collider.transform.TransformPoint(new Vector3(left, bottom, 0f));
            Vector3 bottomRight = collider.transform.TransformPoint(new Vector3(right, bottom, 0f));

            SimplePolygon poly = new SimplePolygon();
            poly.addVertex(new Vector2(bottomLeft.x, bottomLeft.y));
            poly.addVertex(new Vector2(topLeft.x, topLeft.y));
            poly.addVertex(new Vector2(topRight.x, topRight.y));
            poly.addVertex(new Vector2(bottomRight.x, bottomRight.y));

            return poly;
        }
    }
}
