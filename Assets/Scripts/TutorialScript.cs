using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialScript : MonoBehaviour {
    private Transform conversationText;
    private Transform tutorialText;

    private bool isTriggered;
    private bool readConversation;
    private int pageCount;
    private int currentPage;

    public GameObject conversation;
    public GameObject tutorial;

    void Start() {
        isTriggered = false;
        readConversation = false;
        if (conversation != null) {
            conversationText = conversation.transform.GetChild(2);
        }
        tutorialText = tutorial.transform.GetChild(1);
        pageCount = tutorialText.childCount;
        currentPage = 1;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Return)) {
            if (isTriggered) {
                if (!readConversation) {
                    conversation.SetActive(false);
                    tutorial.SetActive(true);
                    readConversation = true;
                } else {
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
    }

    private void OnTriggerStay(Collider other) {
        if (!isTriggered) {
            if (conversation != null) {
                conversation.SetActive(true);
            } else {
                readConversation = true;
                tutorial.SetActive(true);
            }
            isTriggered = true;
        }
    }
}
