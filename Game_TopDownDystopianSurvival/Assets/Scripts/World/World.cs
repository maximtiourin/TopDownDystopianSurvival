using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/*
 * A world encompasses all gameplay mechanics such as levels and other types of game screens.
 *
 * TODO - Change flow so that a world instantiates level components, instead of the level component already existing like it does now.
 */
public class World : MonoBehaviour, Loadable {
    private bool loaded = false;

    private List<Level> levels;

	// Use this for initialization
	void Start () {
        levels = new List<Level>();
	}
	
	// Update is called once per frame
	void Update () {
	    if (isLoaded()) {
            //Frame Timestep
            GameTime.realTime += Time.deltaTime;
        }
	}

    void FixedUpdate () {
        if (isLoaded()) {
            //Logic Timestep
            float gameDelta = GameTime.getSpeedMultiplier() * Time.fixedDeltaTime;
            GameTime.gameTime += gameDelta;
            GameTime.ticks++;

            //TODO Pass gameDelta to custom update functions throughout all levels to properly handle game-logic
        }
    }

    public void load() {
        loaded = true;
    }

    public bool isLoaded() {
        return loaded;
    }
}
