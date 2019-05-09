using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour {
    public GameObject[] dummies;
    public Image fade;

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
    }

    private IEnumerator Fade() {
        fade.GetComponent<Animator>().SetTrigger("Fade");
        yield return new WaitForSecondsRealtime(1.5f);
        SceneManager.LoadScene("SecondScene");
    }
}
