using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameState : MonoBehaviour {
    public GameObject[] players;
    public GameObject[] enemies;
    public GameObject winMenu;
    public GameObject loseMenu;

    private bool gameOver;

    // Start is called before the first frame update
    void Start() {
        gameOver = false;
    }

    // Update is called once per frame
    void Update() {
        if (!gameOver) {
            int count = 4;
            foreach (GameObject enemy in enemies) {
                AttackableGameObject temp = enemy.GetComponentInChildren<AttackableGameObject>();
                if (temp != null && !temp.IsDead()) {
                    count--;
                }
            }
            if (count == 4) {
                gameOver = true;
                Invoke("Win", 3);
            }
        }

        if (!gameOver) {
            int count = 2;
            foreach (GameObject player in players) {
                AttackableGameObject temp = player.GetComponent<AttackableGameObject>();
                if (temp != null && !temp.IsDead()) {
                    count--;
                }
            }
            if (count == 2) {
                gameOver = true;
                Invoke("Lose", 3);
            }
        }
    }

    private void Win() {
        winMenu.SetActive(true);
    }

    private void Lose() {
        loseMenu.SetActive(true);
    }
}
