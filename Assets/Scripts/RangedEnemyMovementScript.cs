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
    private CutsceneManager cutsceneManager;
    private bool lastPauseStatus;
    private GameObject currentTarget;
    private HealthScript healthBar;
    private GameObject autoAttackPrefab;
    private float lastAttackTime;
    private bool isDead;
    private Animator animator;
    private Status status;
    private float statusStartTime;
    private Vector3 pauseVelocity;

    public List<GameObject> players;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponentInParent<NavMeshAgent>();
        pauseManager = PauseManager.Instance;
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        timeManager = TimeManager.Instance;
        cutsceneManager = CutsceneManager.Instance;
        lastPauseStatus = false;
        healthBar = GetComponent<HealthScript>();
        autoAttackPrefab = Resources.Load("EnemyRangedAutoAttack") as GameObject;
        lastAttackTime = -RANGED_ATTACK_CD;
        isDead = false;
        animator = GetComponentInParent<Animator>();
        status = null;
    }

    // Update is called once per frame
    void Update() {
        if (cutsceneManager.CutsceneHappening()) {
            if (agent.remainingDistance < 0.5) {
                animator.SetBool("IsMoving", false);
            } else {
                animator.SetBool("IsMoving", true);
            }
        } else {
            if (pauseManager.IsPaused()) {
                // If we are now paused, and this isn't paused yet
                if (!lastPauseStatus) {
                    lastPauseStatus = true;
                    pauseVelocity = agent.velocity;
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    animator.enabled = false;
                }
            } else {
                // If we aren't paused, but this still is
                if (lastPauseStatus) {
                    lastPauseStatus = false;
                    agent.velocity = pauseVelocity;
                    agent.isStopped = false;
                    animator.enabled = true;
                }

                if (status != null && !IsDead()) {
                    agent.velocity = Vector3.zero;
                    agent.isStopped = true;
                    animator.enabled = false;
                    if (status.type == "push") {
                        Vector3 originToTarget = status.origin - transform.position;
                        Vector3 direction = new Vector3(
                            Vector3.Normalize(originToTarget).x,
                            0,
                            Vector3.Normalize(originToTarget).z);
                        transform.parent.Translate(direction * Time.deltaTime * 15);
                        if (originToTarget.magnitude >= 10) {
                            status = null;
                            animator.enabled = true;
                            agent.isStopped = false;
                        }
                    }

                    if (timeManager.getTimeSeconds() - statusStartTime > status.length) {
                        RemoveStatus();
                    }
                } else {
                    if (currentTarget != null) {
                        AttackableGameObject target = currentTarget.GetComponent<AttackableGameObject>();
                        if (target == null || target.IsDead()) {
                            players.Remove(currentTarget);
                            ChooseTarget();
                        } else {
                            Vector3 playerPosition = currentTarget.transform.position;
                            float distance = (transform.position - playerPosition).magnitude;
                            NavMeshPath path = new NavMeshPath();
                            agent.CalculatePath(playerPosition, path);
                            if (distance > RANGED_DISTANCE || path.corners.Length > 2) {
                                agent.destination = playerPosition;
                                animator.SetBool("IsMoving", true);
                                ChooseTarget();
                            } else {
                                agent.velocity = Vector3.zero;
                                agent.destination = transform.position;
                                if (timeManager.getTimeSeconds() - lastAttackTime > RANGED_ATTACK_CD) {
                                    transform.LookAt(new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z));
                                    transform.Rotate(0, -135, 0);
                                    Attack attack = new Attack("autoattack", gameObject, currentTarget, 10);
                                    GameObject autoAttack = Instantiate(autoAttackPrefab, transform.position + transform.forward, transform.rotation);
                                    autoAttack.GetComponent<RangedAutoAttackProjectile>().setAttack(attack);
                                    lastAttackTime = timeManager.getTimeSeconds();
                                    animator.SetBool("IsMoving", false);
                                    animator.SetTrigger("IsAttacking");
                                }
                            }
                        }
                    } else {
                        ChooseTarget();
                        if (currentTarget == null) {
                            animator.SetBool("IsMoving", false);
                        }
                    }
                }
            }
        }
    }

    public bool IsDead() {
        return isDead;
    }

    public bool IsMoving() {
        return agent.isStopped;
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
        if (attack.GetTarget().Equals(gameObject)) {
            if (healthBar.TakeDamage(attack.GetDamage())) {
                // alive
                if (attack.GetAbility() != null) {
                    attack.GetAbility().DoAbilityEffect(attack.GetOwner(), gameObject);
                }
            } else {
                // dead
                animator.enabled = true;
                animator.SetTrigger("IsDead");
                attackManager.Unsubscribe(this);
                Destroy(gameObject);
                Destroy(this);
            }
        }
    }

    public void SetStatus(Status status)
    {
        this.status = status;
        this.statusStartTime = timeManager.getTimeSeconds();
    }

    public void RemoveStatus()
    {
        this.status = null;
        Debug.Log("Removed status from " + this.gameObject.name);
    }

    public void OnTriggerEnter(Collider other) {
        status = null;
        animator.enabled = true;
        agent.isStopped = false;
    }
}
