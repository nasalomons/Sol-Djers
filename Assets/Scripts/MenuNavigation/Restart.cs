using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Restart : MonoBehaviour
{
    public Button button;

    private void Start() {
        button.onClick.AddListener(Reload);
    }

    private void Reload() {
        SceneManager.LoadScene("SecondScene");
    }
}
