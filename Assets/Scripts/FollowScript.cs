using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowScript : MonoBehaviour {

    public GameObject toFollow;

    void Update() {
        if (transform.position != new Vector3(0, 3, 0) + toFollow.transform.position) {
            transform.position = toFollow.transform.position + new Vector3(0, 3, 0);
        }
    }
}
