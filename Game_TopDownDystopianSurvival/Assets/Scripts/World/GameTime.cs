using UnityEngine;
using System.Collections;

public static class GameTime {
    public static readonly int SPEED_STATE_PAUSED = 0;
    public static readonly int SPEED_STATE_NORMAL = 1;
    public static readonly int SPEED_STATE_FASTER = 2;
    public static readonly int SPEED_STATE_FASTEST = 3;

    //Multiplier for how fast events happen in relation to real-time, EX: 8f multiplier means 8 seconds in-game time happen for every 1 second real-time, etc
    public static readonly float SPEED_NORMAL = 1f;
    public static readonly float SPEED_FASTER = 4f;
    public static readonly float SPEED_FASTEST = 8f;

    //Singleton variables
    public static float gameTime = 0f;
    public static float realTime = 0f;
    public static ulong ticks = 0;
    public static int speedState = SPEED_STATE_PAUSED;
    public static int lastSpeedState = SPEED_STATE_NORMAL;

    public static float getSpeedMultiplier() {
        if (speedState == SPEED_STATE_NORMAL) return SPEED_NORMAL;
        else if (speedState == SPEED_STATE_FASTER) return SPEED_FASTER;
        else if (speedState == SPEED_STATE_FASTEST) return SPEED_FASTEST;
        else return 0f;
    }

    public static void setSpeedState(int s) {
        int state = speedState;
        speedState = s;
        lastSpeedState = state;
    }

    public static void togglePause() {
        if (speedState == SPEED_STATE_PAUSED) {
            setSpeedState(lastSpeedState);
        }
        else {
            setSpeedState(SPEED_STATE_PAUSED);
        }
    }

    public static bool isPaused() {
        return speedState == SPEED_STATE_PAUSED;
    }

    public static string getSpeedStateString(int state) {
        if (state == SPEED_STATE_PAUSED) return "Paused (" + 0f + ")";
        else if (state == SPEED_STATE_NORMAL) return "Normal (" + SPEED_NORMAL + ")";
        else if (state == SPEED_STATE_FASTER) return "Faster (" + SPEED_FASTER + ")";
        else if (state == SPEED_STATE_FASTEST) return "Fastest (" + SPEED_FASTEST + ")";
        else return "";
    }
}
