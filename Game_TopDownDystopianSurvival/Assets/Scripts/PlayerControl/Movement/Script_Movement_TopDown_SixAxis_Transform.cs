using UnityEngine;
using System.Collections;

public class Script_Movement_TopDown_SixAxis_Transform : MonoBehaviour {
    public float scalarMovementSpeed = 1f;

    private float scalarDiagonal = .7071f;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    void FixedUpdate() {
        float ax = Input.GetAxis("Horizontal");
        float ay = Input.GetAxis("Vertical");

        bool horizontal = ax != 0f;
        bool vertical = ay != 0f;

        if (horizontal && vertical) {
            ax *= scalarDiagonal;
            ay *= scalarDiagonal;
        }

        ax *= scalarMovementSpeed;
        ay *= scalarMovementSpeed;

        transform.Translate(ax, ay, 0f);
    }
}
