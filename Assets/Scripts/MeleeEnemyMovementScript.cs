using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;

// Melee enemies will move until they are within melee distance of the player. Once they are in melee distance they stop moving. If the player moves away,
// melee enemies will immediately begin to move towards the player.
public class MeleeEnemyMovementScript : MonoBehaviour {
    private readonly float MELEE_DISTANCE = 2;

    private NavMeshAgent agent;
    private PauseManager pauseManager;
    private bool lastPauseStatus;

    public GameObject player;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        pauseManager = PauseManager.Instance;
        lastPauseStatus = false;
    }

    // Update is called once per frame
    void Update() {
        if (pauseManager.IsPaused()) {
            // If we are now paused, and this isn't paused yet
            if (!lastPauseStatus) {
                lastPauseStatus = true;
                agent.isStopped = true;
            }
        } else {
            // If we aren't paused, but this still is
            if (lastPauseStatus) {
                lastPauseStatus = false;
                agent.isStopped = false;
            }

            Vector3 playerPosition = player.transform.position;
            float distance = (transform.position - playerPosition).magnitude;
            if (distance > MELEE_DISTANCE) {
                agent.destination = playerPosition;
            } else {
                agent.destination = agent.transform.position;
            }
        }
    }
}
