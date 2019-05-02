using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour {
    private bool isTriggered;
    private Transform tutorialText;
    private int pageCount;
    private int currentPage;

    public GameObject tutorial;

    void Start() {
        isTriggered = false;
        tutorialText = tutorial.transform.GetChild(2);
        pageCount = tutorialText.childCount;
        currentPage = 1;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (isTriggered) {
                if (++currentPage < pageCount) {
                    tutorialText.GetChild(currentPage - 1).gameObject.SetActive(false);
                    tutorialText.GetChild(currentPage).gameObject.SetActive(true);
                } else {
                    Destroy(tutorial);
                    Destroy(this);
                }   
            }
        }
    }

    private void OnTriggerStay(Collider other) {
        if (!isTriggered) {
            tutorial.SetActive(true);
            isTriggered = true;
        }
    }
}
