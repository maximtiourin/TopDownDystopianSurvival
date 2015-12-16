using UnityEngine;
using System.Collections;

/**
 * Workaround Utility class to deal with the collision events of other collider objects
 */
public class Collision2DListener : MonoBehaviour {
    public class Event {
        public Collision2D coll;
        public string id = "";

        public Event(Collision2D coll, string id) {
            this.coll = coll;
            this.id = id;
        }
    }

    public GameObject obj;
    public string id = ""; //Optional identifier that can be used to compare values for events

    void OnCollisionEnter2D(Collision2D coll) {
        obj.SendMessage("OnEventCollisionEnter2D", new Event(coll, id), SendMessageOptions.DontRequireReceiver);
    }

    void OnCollisionStay2D(Collision2D coll) {
        obj.SendMessage("OnEventCollisionStay2D", new Event(coll, id), SendMessageOptions.DontRequireReceiver);
    }

    void OnCollisionExit2D(Collision2D coll) {
        obj.SendMessage("OnEventCollisionExit2D", new Event(coll, id), SendMessageOptions.DontRequireReceiver);
    }
}
