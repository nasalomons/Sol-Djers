using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuitToMenu : MonoBehaviour {
    public Button button;

    private void Start() {
        button.onClick.AddListener(Begin);
    }

    private void Begin() {
        SceneManager.LoadScene("Menu");
    }
}
