using UnityEngine;
using System.Collections;

/*
 * A Pawn is an entity in the game world that holds some kind of state such as individual position, rotation, etc, and can potentially be drawn.
 * TODO - LOTS O SHIT
 */
public abstract class Pawn {
    protected GameObject renderObject;
    protected Level level;

    protected long guid;
    
	protected void construct() {
        renderObject = null;
        level = null;

        guid = Identifier.getGlobalUniqueIdentifier();
    }
	
	protected void update() {
	
	}

    //Initialize pawn specific info
    protected abstract void init();

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

    public void setWorldPositionX(float x) {
        if (renderObject != null) {
            Vector3 v = renderObject.transform.position;
            renderObject.transform.position = new Vector3(x, v.y, v.z);
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

    public void setWorldPositionY(float y) {
        if (renderObject != null) {
            Vector3 v = renderObject.transform.position;
            renderObject.transform.position = new Vector3(v.x, y, v.z);
        }
    }

    public Vector3 getWorldPosition() {
        if (renderObject != null) {
            return renderObject.transform.position;
        }
        else {
            return Vector3.zero;
        }
    }

    public void setWorldPosition(Vector3 v) {
        if (renderObject != null) {
            renderObject.transform.position = new Vector3(v.x, v.y, v.z);
        }
    }

    public void setWorldPosition(float x, float y) {
        if (renderObject != null) {
            Vector3 v = renderObject.transform.position;
            renderObject.transform.position = new Vector3(x, y, v.z);
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
