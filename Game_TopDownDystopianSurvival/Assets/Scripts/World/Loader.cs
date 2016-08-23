using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * Handles all world loading and generation synchronization so everything happens in the correct order
 */
public class Loader : MonoBehaviour {
    private bool loaded;
    private Loadable currentLoading;
    private Queue<Loadable> loadables;
    private Level level;
    private Component_TileDataGenerator tileDataGenerator;

	// Use this for initialization
	void Start () {
        loaded = false;
        currentLoading = null;
        loadables = new Queue<Loadable>();

        tileDataGenerator = gameObject.GetComponent<Component_TileDataGenerator>();
        loadables.Enqueue(tileDataGenerator);
        level = gameObject.GetComponent<Level>();
        loadables.Enqueue(level);
    }
	
	// Update is called once per frame
	void Update () {
	    if (!loaded) {
            if (currentLoading == null) {
                if (loadables.Count > 0) {
                    currentLoading = loadables.Dequeue();
                    
                    if (!currentLoading.isLoaded()) {
                        currentLoading.load();
                    }
                    else {
                        loaded = true;
                    }
                }
                else {
                    loaded = true;
                }
            }
            else {
                if (currentLoading.isLoaded()) {
                    currentLoading = null;
                }
            }
        }
	}
}
