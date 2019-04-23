using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyScript : MonoBehaviour, AttackableGameObject {
    private AttackManager attackManager;
    private int count;

    void Start() {
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        count = 0;
    }

    public void OnAttacked(AttackManager.Attack attack) {
        Debug.Log("dummy was attacked " + ++count);
    }
}
