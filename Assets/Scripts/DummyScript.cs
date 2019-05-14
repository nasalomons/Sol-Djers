using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class DummyScript : MonoBehaviour, AttackableGameObject {
    private AttackManager attackManager;
    private PauseManager pauseManager;
    private HealthScript healthBar;
    private bool isDead;
    private Animator animator;
    private Status status;

    void Start() {
        attackManager = AttackManager.Instance;
        pauseManager = PauseManager.Instance;
        attackManager.Subscribe(this);
        healthBar = gameObject.GetComponent<HealthScript>();
        isDead = false;
        animator = GetComponentInParent<Animator>();
        status = null;
    }

    void Update() {
        if (pauseManager.IsPaused()) {
            animator.enabled = false;
        } else {
            animator.enabled = true;
            if (status != null) {
                animator.enabled = false;
                if (status.type == "push") {
                    Vector3 originToTarget = status.origin - transform.position;
                    Vector3 direction = new Vector3(
                        Vector3.Normalize(originToTarget).x,
                        0,
                        Vector3.Normalize(originToTarget).z);
                    transform.parent.Translate(direction * Time.deltaTime * 10);
                    if (originToTarget.magnitude >= 10) {
                        status = null;
                        animator.enabled = true;
                    }
                }
            }
        }
    }

    public void OnAttacked(AttackManager.Attack attack) {
        if (attack.GetTarget() == gameObject) {
            if (healthBar.TakeDamage(attack.GetDamage())) {
                if (attack.GetAbility() != null) {
                    attack.GetAbility().DoAbilityEffect(attack.GetOwner(), gameObject);
                }
            } else {
                isDead = true;
                attackManager.Unsubscribe(this);
                animator.SetTrigger("Die");
            }            
        }
    }

    public bool IsDead() {
        return isDead;
    }

    public void SetStatus(Status status) {
        this.status = status;
    }

    public void OnTriggerEnter(Collider other) {
        status = null;
        animator.enabled = true;
    }
}
