using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* This EventManager is a singleton class that manages all the actions all characters want to do. All characters must get permission from this to perform the action. This is necessary
 * because it allows the user to buffer actions while paused and have them occur when unpausing. */
public sealed class EventManager : MonoBehaviour {

    // The Action class represents some action a character can take.
    // Each action has a name (the type of action), an owner (who wants to do the action), and other data maybe needed to perform the action.
    public class Action {
        private string name;
        private ActionableGameObject owner;
        private RaycastHit destination;
        private Action nextAction;

        public Action(string name, ActionableGameObject owner) {
            this.name = name;
            this.owner = owner;
        }

        public Action(string name, ActionableGameObject owner, RaycastHit destination) : this(name, owner) {
            this.destination = destination;
        }

        public Action(string name, ActionableGameObject owner, RaycastHit destination, Action nextAction) : this(name, owner, destination) {
            this.nextAction = nextAction;
        }

        public string getName() { return name; }
        public ActionableGameObject getOwner() { return owner; }
        public RaycastHit getDestination() { return destination; }
        public Action getNextAction() { return nextAction; }
    }

    // Creating event with parameter
    private class ActionEvent : UnityEvent<Action> { }

    // Singleton of this event manager
    private static EventManager eventManager;
    // Queue of actions to send out
    private List<Action> actionQueue;
    // List of objects listening to the event system and their corresponding event
    private Dictionary<ActionableGameObject, ActionEvent> eventMap;
    private PauseManager pauseManager;

    public static EventManager Instance {
        get {
            if (eventManager == null) {
                eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
                eventManager.SetUp();
            }

            return eventManager;
        }
    }

    private void SetUp() {
        actionQueue = new List<Action>();
        eventMap = new Dictionary<ActionableGameObject, ActionEvent>();
        pauseManager = PauseManager.Instance;
    }

    public bool QueueAction(Action action) {
        // If the owner of the action isn't subscribed, don't enqueue it
        if (action == null || !eventMap.ContainsKey(action.getOwner())) {
            return false;
        }

        // Remove other actions of the owner from the queue (cancels them)
        List<int> indexList = new List<int>();
        foreach (Action queued in actionQueue) {
            if (queued.getOwner().Equals(action.getOwner())) {
                indexList.Add(actionQueue.IndexOf(queued));
            }
        }
        foreach (int index in indexList) {
            actionQueue.RemoveAt(index);
        }
        
        // Add action to the end of the queue
        actionQueue.Add(action);
        return true;
    }

    // If the parameter is not already in the map, adds the ActionableGameObject-ActionEvent pair to the map with a new ActionEvent. Then subscribes the ActionableGameObject to the ActionEvent.
    public void Subscribe(ActionableGameObject owner) {
        ActionEvent actionEvent;
        if (!eventMap.ContainsKey(owner)) {
            actionEvent = new ActionEvent();
            eventMap.Add(owner, actionEvent);

            UnityAction<Action> callback = owner.OnActionEvent;
            actionEvent.AddListener(callback);
        }
    }

    // If the parameter is in the map, removes the mapping from the map and unsubscribes the ActionableGameObject.
    public void Unsubscribe(ActionableGameObject owner) {
        if (eventMap.ContainsKey(owner)) {
            ActionEvent actionEvent;
            eventMap.TryGetValue(owner, out actionEvent);
            eventMap.Remove(owner);

            UnityAction<Action> callback = owner.OnActionEvent;
            actionEvent.RemoveListener(callback);
        }
    }

    void Update() {
        if (!pauseManager.IsPaused() && actionQueue.Count > 0) {
            Action action = actionQueue[0];
            actionQueue.Remove(action);

            ActionEvent actionEvent;
            eventMap.TryGetValue(action.getOwner(), out actionEvent);

            if (actionEvent != null) {
                actionEvent.Invoke(action);
            }
        }
    }
}
