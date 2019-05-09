using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/* Singleton class that manages the paused/unpaused state of the game. */
public sealed class PauseManager : MonoBehaviour {
    public GameObject pausedText;

    private static PauseManager pauseManager;
    private bool paused;

    public static PauseManager Instance {
        get {
            if(pauseManager == null) {
                pauseManager = FindObjectOfType(typeof(PauseManager)) as PauseManager;  
                pauseManager.SetUp();
            }

            return pauseManager;
        }
    }

    private void SetUp() {
        paused = false;
    }

    public bool IsPaused() {
        return paused;
    }

    public void TogglePause() {
        if (!CutsceneManager.Instance.CutsceneHappening()) {
            paused = paused ? false : true;
            if (paused) {
                pausedText.SetActive(true);
            } else {
                pausedText.SetActive(false);
            }
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Space)) {
            TogglePause();             
        }
    }
}
