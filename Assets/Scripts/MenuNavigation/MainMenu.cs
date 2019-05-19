using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour {
    public Button start;
    public Button quit;

    private void Start() {
        start.onClick.AddListener(Begin);
        quit.onClick.AddListener(Quit);
    }

    private void Begin() {
        SceneManager.LoadScene("MainScene");
    }

    private void Quit() {
        Application.Quit();
    }
}
