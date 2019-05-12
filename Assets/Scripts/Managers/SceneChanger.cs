using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour {
    public GameObject[] dummies;
    public Image fade;
    public GameObject menu;

    private bool lastPauseState;

    // Update is called once per frame
    void Update() {
        int count = 0;
        foreach (GameObject dummy in dummies) {
            if (dummy.GetComponentInChildren<DummyScript>().IsDead()) {
                count++;
            }
        }
        if (count == 2) {
            StartCoroutine("Fade");
        }

        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (menu.activeSelf) {
                menu.SetActive(false);
                PauseManager.Instance.SetPause(lastPauseState);
            } else {
                menu.SetActive(true);
                lastPauseState = PauseManager.Instance.IsPaused();
                PauseManager.Instance.SetPause(true);
            }
        }
    }

    private IEnumerator Fade() {
        fade.GetComponent<Animator>().SetTrigger("Fade");
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene("SecondScene");
    }
}
