using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Status {
    public string type;
    public Vector3 origin;
    public float length;

    public Status(string type, Vector3 origin) {
        this.type = type;
        this.origin = origin;
    }

    public Status(string type, float length) {
        this.type = type;
        this.length = length;
    }
}
