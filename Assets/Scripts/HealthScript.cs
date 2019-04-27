﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthScript : MonoBehaviour {
    public float maxHealth;

    private float currentHealth;
    private GameObject overheadPrefab;
    private GameObject overhead;
    private Sprite moveSprite;
    private Sprite attackSprite;

    public enum CurrentAction {
        NONE, MOVE, ATTACK
    };

    void Start() {
        currentHealth = maxHealth;
        overheadPrefab = Resources.Load("Overhead") as GameObject;
        moveSprite = Resources.Load<Sprite>("MoveSprite") as Sprite;
        attackSprite = Resources.Load<Sprite>("AttackSprite") as Sprite;
        overhead = Instantiate(overheadPrefab, gameObject.transform.position + new Vector3(0, 4.35f, 0), Quaternion.identity);
        overhead.transform.rotation = Camera.main.transform.rotation;
    }

    void Update() {
        overhead.transform.position = gameObject.transform.position + new Vector3(0, 4f, 0);
        if (currentHealth == maxHealth && !gameObject.tag.Equals("Ally")) {
            overhead.SetActive(false);
        } else {
            overhead.SetActive(true);
            overhead.transform.GetChild(0).GetChild(2).gameObject.transform.localScale = new Vector3(currentHealth/maxHealth, 0.15f, 1);
        }

    }

    // Object takes damage. Returns false if the health is 0 and true otherwise.
    public bool TakeDamage(float amount) {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        return currentHealth != 0;
    }

    public void updateAction(CurrentAction action) {
        Image status = overhead.transform.GetChild(1).GetChild(2).GetComponent<Image>();
        switch (action) {
            case CurrentAction.NONE:
                overhead.transform.GetChild(1).gameObject.SetActive(false);
                break;
            case CurrentAction.MOVE:
                status.sprite = moveSprite;
                overhead.transform.GetChild(1).gameObject.SetActive(true);
                break;
            case CurrentAction.ATTACK:
                status.sprite = attackSprite;
                overhead.transform.GetChild(1).gameObject.SetActive(true);
                break;
        }
    }
}