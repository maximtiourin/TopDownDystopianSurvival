using UnityEngine;
using System.Collections;

public class Script_Camera_LevelView_Constrictor : MonoBehaviour {
    public GameObject levelContainer;
    public int boundaryTiles;
    private Level level;

	// Use this for initialization
	void Start () {
        level = levelContainer.GetComponent<Level>();
	}
	
	// Update is called once per frame
	void Update() {
        Vector3 position = this.transform.position;
        position.x = Mathf.Clamp(position.x, levelContainer.transform.position.x - boundaryTiles, levelContainer.transform.position.x + level.getWidth() + boundaryTiles);
        position.y = Mathf.Clamp(position.y, levelContainer.transform.position.y - boundaryTiles, levelContainer.transform.position.y + level.getHeight() + boundaryTiles);
        this.transform.position = position;
    }
}
