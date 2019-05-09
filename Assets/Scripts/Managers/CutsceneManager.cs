using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CutsceneManager : MonoBehaviour {
    private static CutsceneManager cutsceneManager;
    private bool inCutscene;
    private Vector3[] positions;
    private bool teacherDead;

    public GameObject teacher;
    public GameObject[] enemies;
    public Image fade;

    public static CutsceneManager Instance {
        get {
            if (cutsceneManager == null) {
                cutsceneManager = FindObjectOfType(typeof(CutsceneManager)) as CutsceneManager;
                cutsceneManager.SetUp();
            }

            return cutsceneManager;
        }
    }

    private void SetUp() {
        if (teacher != null) {
            fade.GetComponent<Animator>().SetTrigger("Fade");
        }
        inCutscene = true;
        teacherDead = false;
        positions = new Vector3[] {
            new Vector3(51, 0, 68),
            new Vector3(42, 0, 68),
            new Vector3(47, 0, 74),
            new Vector3(55, 0, 74)
        };
        if (teacher != null) {
            Invoke("EndCutscene", 12);
        }
    }

    // Update is called once per frame
    void Update() {
        if (teacher == null) {
            inCutscene = false;
        } else {
            if (inCutscene) {
                enemies[1].GetComponentInChildren<MeleeEnemyMovementScript>().SetTarget(teacher);
                for (int i = 0; i < enemies.Length; i++) {
                    if (i != 1 || teacherDead) {
                        enemies[i].GetComponent<NavMeshAgent>().destination = positions[i];
                    }
                }
            }
        }
    }

    private void EndCutscene() {
        inCutscene = false;
        PauseManager.Instance.TogglePause();
    }

    public bool CutsceneHappening() {
        return inCutscene;
    }

    public void KillTeacher() {
        teacherDead = true;
    }
}
