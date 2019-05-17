using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static AttackManager;
using static EventManager;

// Melee enemies will move until they are within melee distance of the player. Once they are in melee distance they stop moving. If the player moves away,
// melee enemies will immediately begin to move towards the player.
public class MeleeEnemyMovementScript : MonoBehaviour, AttackableGameObject {
    private readonly float MELEE_DISTANCE = 4;
    private readonly float MELEE_ATTACK_CD = 2.5f;

    private NavMeshAgent agent;
    private PauseManager pauseManager;
    private AttackManager attackManager;
    private TimeManager timeManager;
    private CutsceneManager cutsceneManager;
    private bool lastPauseStatus;
    private GameObject currentTarget;
    private HealthScript healthBar;
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
        healthBar = gameObject.GetComponent<HealthScript>();
        lastAttackTime = -MELEE_ATTACK_CD;
        isDead = false;
        animator = GetComponentInParent<Animator>();
        status = null;
    }

    // Update is called once per frame
    void Update() {
        if (cutsceneManager.CutsceneHappening()) {
            if (currentTarget != null) {
                AttackTarget();
            }
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

                if (status != null) {
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
                } else {
                    if (currentTarget != null) {
                        AttackableGameObject target = currentTarget.GetComponent<AttackableGameObject>();
                        if (target == null || target.IsDead()) {
                            players.Remove(currentTarget);
                            ChooseTarget();
                        } else {
                            AttackTarget();
                        }
                    } else {
                        ChooseTarget();
                        if (currentTarget == null) {
                            animator.SetBool("IsMoving", false);
                        }
                    }
                }

                if (status != null && (timeManager.getTimeSeconds() - statusStartTime > status.length))
                {
                    RemoveStatus();
                }
            }
        }
    }

    private void AttackTarget() {
        Vector3 playerPosition = currentTarget.transform.position;
        float distance = (transform.position - playerPosition).magnitude;
        if (distance > MELEE_DISTANCE) {
            agent.destination = playerPosition;
            animator.SetBool("IsMoving", true);
            ChooseTarget();
        } else {
            agent.velocity = Vector3.zero;
            agent.destination = transform.position;
            transform.LookAt(new Vector3(currentTarget.transform.position.x, transform.position.y, currentTarget.transform.position.z));
            if (timeManager.getTimeSeconds() - lastAttackTime > MELEE_ATTACK_CD) {
                Attack attack = new Attack("autoattack", gameObject, currentTarget, 10);
                attackManager.QueueAttack(attack);
                lastAttackTime = timeManager.getTimeSeconds();
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsAttacking");
                if (cutsceneManager.CutsceneHappening()) {
                    Invoke("KillTeacher", 2.5f);
                }
            }
        }
    }

    private void KillTeacher() {
        cutsceneManager.KillTeacher();
    }

    public bool IsDead() {
        return isDead;
    }

    public void SetTarget(GameObject target) {
        currentTarget = target;
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
                animator.SetTrigger("IsDead");
                attackManager.Unsubscribe(this);
                Destroy(gameObject);
                Destroy(this);
            }
        }
    }

    public void SetStatus(Status status) {
        this.status = status;
        this.statusStartTime = timeManager.getTimeSeconds();
    }

    public void RemoveStatus()
    {
        this.status = null;
        Debug.Log("Removed status from " + this.gameObject.name);

        foreach (Transform child in transform)
        {
            if (child.gameObject.name == "BeveledStar(Clone)")
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider other) {
        status = null;
        animator.enabled = true;
        agent.isStopped = false;
    }
}
