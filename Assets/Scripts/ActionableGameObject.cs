using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static EventManager;

/* This represents some GameObject that can send actions to the EventManager and do them. */
public interface ActionableGameObject {
    // Callback for EventManager events
    void OnActionEvent(Action action); 
}
