using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour, AttackableGameObject {
    private AttackManager attackManager;
    private int count;
    private HealthScript healthBar;
    private bool isDead;

    void Start() {
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        count = 0;
        healthBar = gameObject.GetComponent<HealthScript>();
        isDead = false;
    }

    public void OnAttacked(AttackManager.Attack attack) {
        if (attack.getTarget() == gameObject) {
            Debug.Log("dummy was attacked " + ++count);
            if (healthBar.TakeDamage(attack.getDamage())) {
                if (attack.getAbility() != null) {
                    attack.getAbility().DoAbilityEffect(gameObject);
                }
            } else {
                isDead = true;
            }            
        }
    }

    public bool IsDead() {
        return isDead;
    }
}
