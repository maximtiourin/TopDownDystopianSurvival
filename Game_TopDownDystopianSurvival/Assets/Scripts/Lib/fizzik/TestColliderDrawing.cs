using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using FizzikLib;

public class TestColliderDrawing : MonoBehaviour {
    private BoxCollider2D coll;
    private SimplePolygon poly;

	// Use this for initialization
	void Start () {
        coll = GetComponent<BoxCollider2D>();
        poly = SimplePolygon.boxCollider2DToSimplePolygon(coll);
    }
	
	// Update is called once per frame
	void Update () {
        if (transform.hasChanged) {
            poly = SimplePolygon.boxCollider2DToSimplePolygon(coll);
        }
    }

    void OnDrawGizmosSelected() {
        List<Vector2> vs = poly.getVertices();
        Vector2 a, b;

        Gizmos.color = Color.red;

        a = vs[0];
        b = vs[1];
        Gizmos.DrawLine(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0));
        a = vs[1];
        b = vs[2];
        Gizmos.DrawLine(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0));
        a = vs[2];
        b = vs[3];
        Gizmos.DrawLine(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0));
        a = vs[3];
        b = vs[0];
        Gizmos.DrawLine(new Vector3(a.x, a.y, 0), new Vector3(b.x, b.y, 0));
    }
}
