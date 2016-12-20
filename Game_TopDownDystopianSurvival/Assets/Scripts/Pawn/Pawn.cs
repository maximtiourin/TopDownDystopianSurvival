using UnityEngine;
using System.Collections;

/*
 * A Pawn is an entity in the game world that holds some kind of state such as individual position, rotation, etc, and can potentially be drawn.
 * TODO - LOTS O SHIT
 */
public abstract class Pawn : MonoBehaviour {
    public static long GUID = 0;

    protected GameObject renderObject;
    protected Level level;

    protected long guid;

	// Use this for initialization
	void Start () {
        renderObject = null;
        level = null;

        guid = ++Pawn.GUID;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    /*
     * Can only be called once to generate the initial gameObject, which should be
     * modified after that.
     */
    public void generateRenderObject() {
        if (renderObject == null) {
            renderObject = new GameObject();
            renderObject.name = "Pawn_" + guid;
            renderObject.transform.parent = level.getPawnObjectContainer().transform;
        }
    }

    /*
     * Raw world position x based on transform of renderObject
     */
    public float getWorldPositionX() {
        if (renderObject != null) {
            return renderObject.transform.position.x;
        }
        else {
            return 0f;
        }
    }

    /*
     * Raw world position y based on transform of renderObject
     */
    public float getWorldPositionY() {
        if (renderObject != null) {
            return renderObject.transform.position.y;
        }
        else {
            return 0f;
        }
    }

    public void setLevelContainer(Level level) {
        this.level = level;
    }

    public Level getLevelContainer() {
        return level;
    }

    public long getGUID() {
        return guid;
    }
}
