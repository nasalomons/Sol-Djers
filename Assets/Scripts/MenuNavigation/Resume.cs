using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Resume : MonoBehaviour {
    public Button button;
    public GameObject parent;

    private void Start() {
        button.onClick.AddListener(Close);
    }

    private void Close() {
        parent.SetActive(false);
    }
}
