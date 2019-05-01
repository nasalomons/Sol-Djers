using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AttackManager;
using static EventManager;

// Melee enemies will move until they are within melee distance of the player. Once they are in melee distance they stop moving. If the player moves away,
// melee enemies will immediately begin to move towards the player.
public class MeleeEnemyMovementScript : MonoBehaviour, AttackableGameObject {
    private readonly float MELEE_DISTANCE = 2;
    private readonly float MELEE_ATTACK_CD = 1.5f;

    private NavMeshAgent agent;
    private PauseManager pauseManager;
    private AttackManager attackManager;
    private TimeManager timeManager;
    private bool lastPauseStatus;
    private GameObject currentTarget;
    private HealthScript healthBar;
    private float lastAttackTime;
    private bool isDead;

    public List<GameObject> players;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        pauseManager = PauseManager.Instance;
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        timeManager = TimeManager.Instance;
        lastPauseStatus = false;
        ChooseTarget();
        healthBar = gameObject.GetComponent<HealthScript>();
        lastAttackTime = -MELEE_ATTACK_CD;
        isDead = false;
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

            if (currentTarget != null) {
                if (currentTarget.GetComponent<AttackableGameObject>().IsDead()) {
                    players.Remove(currentTarget);
                    ChooseTarget();
                } else {
                    Vector3 playerPosition = currentTarget.transform.position;
                    float distance = (transform.position - playerPosition).magnitude;
                    if (distance > MELEE_DISTANCE) {
                        agent.destination = playerPosition;
                        ChooseTarget();
                    } else {
                        agent.destination = agent.transform.position;
                        if (timeManager.getTimeSeconds() - lastAttackTime > MELEE_ATTACK_CD) {
                            Attack attack = new Attack("autoattack", gameObject, currentTarget, 10);
                            attackManager.QueueAttack(attack);
                            lastAttackTime = timeManager.getTimeSeconds();
                        }
                    }
                }
            }
        }
    }

    public bool IsDead() {
        return isDead;
    }


    private void ChooseTarget() {
        float currentBest = int.MaxValue;
        foreach (GameObject player in players) {
            float distance = (player.transform.position - transform.position).magnitude;
            if (distance < currentBest) {
                currentTarget = player;
                currentBest = distance;
            }
        }
    }

    public void OnAttacked(AttackManager.Attack attack) {
        if (attack.getTarget().Equals(gameObject)) {
            if (healthBar.TakeDamage(attack.getDamage())) {
                // alive
            } else {
                // dead
                attackManager.Unsubscribe(this);
                Destroy(gameObject);
                Destroy(this);
            }
        }
    }
}
