using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorScript : MonoBehaviour {

    public GameObject[] characters;

    private Animator animator;

    void Start() {
        animator = GetComponent<Animator>();
    }

    void Update() {
        foreach (GameObject character in characters) {
            Vector3 distance = character.transform.position - transform.position;
            if (distance.magnitude <= 7) {
                animator.SetBool("character_nearby", true);
                return;
            }
        }
        animator.SetBool("character_nearby", false);
    }
}
