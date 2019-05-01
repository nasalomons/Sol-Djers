using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AttackManager;
using static EventManager;

// Ranged enemies will move towards the player until they are within ranged distance AND have line of sight.
public class RangedEnemyMovementScript : MonoBehaviour, AttackableGameObject {
    private readonly float RANGED_DISTANCE = 10;
    private readonly float RANGED_ATTACK_CD = 1.5f;

    private NavMeshAgent agent;
    private PauseManager pauseManager;
    private AttackManager attackManager;
    private TimeManager timeManager;
    private bool lastPauseStatus;
    private GameObject currentTarget;
    private HealthScript healthBar;
    private GameObject autoAttackPrefab;
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
        healthBar = GetComponent<HealthScript>();
        autoAttackPrefab = Resources.Load("RangedAutoAttack") as GameObject;
        lastAttackTime = -RANGED_ATTACK_CD;
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
                    NavMeshPath path = new NavMeshPath();
                    agent.CalculatePath(playerPosition, path);
                    if (distance > RANGED_DISTANCE || path.corners.Length > 2) {
                        agent.destination = playerPosition;
                        ChooseTarget();
                    } else {
                        agent.destination = agent.transform.position;
                        if (timeManager.getTimeSeconds() - lastAttackTime > RANGED_ATTACK_CD) {
                            transform.LookAt(currentTarget.transform);
                            Attack attack = new Attack("autoattack", gameObject, currentTarget, 10);
                            GameObject autoAttack = Instantiate(autoAttackPrefab, transform.position + transform.forward * 1.5f, transform.rotation);
                            autoAttack.GetComponent<RangedAutoAttackProjectile>().setAttack(attack);
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
