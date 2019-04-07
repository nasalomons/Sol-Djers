using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Singleton class that manages the paused/unpaused state of the game. */
public sealed class PauseManager : MonoBehaviour {
    private static readonly PauseManager INSTANCE = new PauseManager();

    private bool paused;

    private PauseManager() {
        paused = false;
    }

    public static PauseManager getInstance() {
        return INSTANCE;
    }

    public bool isPaused() {
        return paused;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            paused = paused ? false : true;
        }
    }
}
