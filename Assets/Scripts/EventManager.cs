using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* This EventManager is a singleton class that manages all the actions all characters want to do. All characters must get permission from this to perform the action. This is necessary
 * because it allows the user to buffer actions while paused and have them occur when unpausing. */
public sealed class EventManager : MonoBehaviour {

    // The Action class represents some action a character can take.
    // Each action has a name (the type of action), an owner (who wants to do the action), and other data needed to perform the action.
    public class Action {
        private string name;
        private GameObject owner;
        private Object data;

        public Action(string name, GameObject owner, Object data) {
            this.name = name;
            this.owner = owner;
            this.data = data;
        }

        public string getName() { return name; }
        public GameObject getOwner() { return owner; }
        public Object getData() { return data; }
    }

    // Creating event with parameter
    private class actionEvent : UnityEvent<Action> {}

    // Singleton of this event manager
    private static readonly EventManager INSTANCE = new EventManager();
    // Queue of actions to send out
    private List<Action> actionQueue;
    // List of objects listening to the event system
    private HashSet<GameObject> subscribers;
    private PauseManager pauseManager;

    private EventManager() {
        actionQueue = new List<Action>();
        subscribers = new HashSet<GameObject>();
        pauseManager = PauseManager.getInstance();
    }

    public static EventManager getInstance() {
        return INSTANCE;
    }

    public bool QueueAction(Action action) {
        // If the owner of the action isn't subscribed, don't enqueue it
        if (!subscribers.Contains(action.getOwner())) {
            return false;
        }

        // Remove other actions of the owner from the queue (cancels them)
        foreach (Action queued in actionQueue) {
            if (queued.getOwner().Equals(action.getOwner())) {
                actionQueue.Remove(queued);
            }
        }
        
        // Add action to the end of the queue
        actionQueue.Add(action);
        return true;
    }

    // Adds a GameObject to the subscribers set
    public void Subscribe(GameObject owner) {
        if (!subscribers.Contains(owner)) {
            subscribers.Add(owner);
        }
    }

    // Removes a GameObject to the subscribers set
    public void Unsubscribe(GameObject owner) {
        if (subscribers.Contains(owner)) {
            subscribers.Remove(owner);
        }
    }


}
