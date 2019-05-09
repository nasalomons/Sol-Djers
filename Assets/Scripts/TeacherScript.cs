using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class TeacherScript : MonoBehaviour, AttackableGameObject {
    private bool isDead;
    private Animator animator;
    private AttackManager attackManager;

    void Start() {
        isDead = false;
        animator = GetComponent<Animator>();
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
    }

    void Update() {
        
    }

    public bool IsDead() {
        return isDead;
    }

    public void OnAttacked(AttackManager.Attack attack) {
        animator.SetTrigger("IsDead");
        isDead = true;
        attackManager.Unsubscribe(this);
        Destroy(GetComponent<NavMeshObstacle>());
    }

}
