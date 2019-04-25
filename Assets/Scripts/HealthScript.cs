using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthScript : MonoBehaviour {
    public float maxHealth;

    private float currentHealth;
    private GameObject healthBarPrefab;
    private GameObject healthBar;

    void Start() {
        currentHealth = maxHealth;
        healthBarPrefab = Resources.Load("HealthBar") as GameObject;
        healthBar = Instantiate(healthBarPrefab, gameObject.transform.position + new Vector3(0, 4.35f, 0), Quaternion.identity);
        healthBar.transform.rotation = Camera.main.transform.rotation;
    }

    void Update() {
        healthBar.transform.position = gameObject.transform.position + new Vector3(0, 4.35f, 0);
        if (currentHealth == maxHealth) {
            healthBar.SetActive(false);
        } else {
            healthBar.SetActive(true);
            healthBar.transform.GetChild(2).gameObject.transform.localScale = new Vector3(currentHealth/maxHealth, 0.15f, 1);
        }

    }

    // Object takes damage. Returns false if the health is 0 and true otherwise.
    public bool TakeDamage(float amount) {
        currentHealth = Mathf.Max(0, currentHealth - amount);
        return currentHealth != 0;
    }
}
