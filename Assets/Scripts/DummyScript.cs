using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour, AttackableGameObject {
    private AttackManager attackManager;
    private HealthScript healthBar;
    private bool isDead;
    private Animator animator;

    void Start() {
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        healthBar = gameObject.GetComponent<HealthScript>();
        isDead = false;
        animator = GetComponentInParent<Animator>();
    }

    public void OnAttacked(AttackManager.Attack attack) {
        if (attack.getTarget() == gameObject) {
            if (healthBar.TakeDamage(attack.getDamage())) {
                if (attack.getAbility() != null) {
                    attack.getAbility().DoAbilityEffect(gameObject);
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
}
