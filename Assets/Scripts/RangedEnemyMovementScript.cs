using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;

// Ranged enemies will move towards the player until they are within ranged distance AND have line of sight.
public class RangedEnemyMovementScript : MonoBehaviour {
    private readonly float RANGED_DISTANCE = 10;

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
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(playerPosition, path);
            if (distance > RANGED_DISTANCE || path.corners.Length > 2) {
                agent.destination = playerPosition;
            } else {
                agent.destination = agent.transform.position;
            }         
        }
    }
}
