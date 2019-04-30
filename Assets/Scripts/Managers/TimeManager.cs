using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

/* TimeManager keeps track of time so characters can manage cooldowns and we can standardize time easily. Pauses when the game is paused. */
public sealed class TimeManager : MonoBehaviour {
    private static TimeManager timeManager;
    private PauseManager pauseManager;
    private Stopwatch stopwatch;

    public static TimeManager Instance {
        get {
            if (timeManager == null) {
                timeManager = FindObjectOfType(typeof(TimeManager)) as TimeManager;
                timeManager.SetUp();
            }

            return timeManager;
        }
    }

    private void SetUp() {
        stopwatch = new Stopwatch();
        stopwatch.Start();
        pauseManager = PauseManager.Instance;
    }

    public long getTimeSeconds() {
        return stopwatch.ElapsedMilliseconds / 1000;
    }

    private void Update() {
        if (pauseManager.IsPaused()) {
            stopwatch.Stop();
        } else {
            stopwatch.Start();
        }
    }
}
