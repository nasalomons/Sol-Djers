using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// Melee enemies will move until they are within melee distance of the player. Once they are in melee distance they stop moving. If the player moves away,
// melee enemies will immediately begin to move towards the player.
public class MeleeEnemyMovementScript : MonoBehaviour {
    private float MELEE_DISTANCE = 2;

    private NavMeshAgent agent;

    public GameObject player;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update() {
        float distance = (transform.position - player.transform.position).magnitude;
        if (distance > MELEE_DISTANCE) {
            agent.destination = player.transform.position;
        } else {
            agent.ResetPath();
            
        }
    }
}
