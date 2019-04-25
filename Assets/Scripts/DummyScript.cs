using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour, AttackableGameObject {
    private AttackManager attackManager;
    private int count;
    private HealthScript healthBar;

    void Start() {
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        count = 0;
        healthBar = gameObject.GetComponent<HealthScript>();
    }

    public void OnAttacked(AttackManager.Attack attack) {
        if (attack.getTarget() == gameObject) {
            Debug.Log("dummy was attacked " + ++count);
            healthBar.TakeDamage(attack.getDamage());
        }
    }
}
