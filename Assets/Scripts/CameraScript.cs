using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour {

    public GameObject player;

    // Update is called once per frame
    void Update() {
        transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 20, player.transform.position.z - 9);
    }

    public void SetPlayer(GameObject player) {
        this.player = player;
    }
}
