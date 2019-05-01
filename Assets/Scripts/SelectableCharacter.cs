using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SelectableCharacter : MonoBehaviour
{
    bool selected;

    public void SetSelected(bool val)
    {
        selected = val;
    }

    public bool GetSelected()
    {
        return selected;
    }
}
