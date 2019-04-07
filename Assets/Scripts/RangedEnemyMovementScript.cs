using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Ranged enemies will move towards the player until they are within ranged distance AND have line of sight.
public class RangedEnemyMovementScript : MonoBehaviour {
    private float RANGED_DISTANCE = 10;

    private NavMeshAgent agent;

    public GameObject player;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update() {
        Vector3 playerPosition = player.transform.position;
        float distance = (transform.position - playerPosition).magnitude;
        NavMeshPath path = new NavMeshPath();
        agent.CalculatePath(playerPosition, path);
        if (distance > RANGED_DISTANCE || path.corners.Length > 2) {
            agent.destination = playerPosition;
        } else {
            agent.ResetPath();

        }
    }
}
