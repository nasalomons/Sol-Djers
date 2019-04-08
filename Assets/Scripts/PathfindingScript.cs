using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;

public class PathfindingScript : MonoBehaviour, ActionableGameObject {
    
    private NavMeshAgent agent;
    private GameObject targetIndicatorPrefab;
    private GameObject targetIndicator;
    private EventManager eventManager;
    private PauseManager pauseManager;
    private bool lastPauseStatus;

    public LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        targetIndicatorPrefab = Resources.Load("TargetPoint") as GameObject;
        targetIndicator = null;
        eventManager = EventManager.Instance;
        eventManager.Subscribe(this);
        pauseManager = PauseManager.Instance;
        lastPauseStatus = false;
    }

    void OnDisable() {
        eventManager.Unsubscribe(this);
    }

    // Update is called once per frame
    void Update() {

        if (pauseManager.IsPaused()) {
            // If we are now paused, and this isn't paused yet
            if (!lastPauseStatus) {
                lastPauseStatus = true;
                agent.isStopped = true;
            }
        } else {
            // If we aren't paused, but this still is
            if (lastPauseStatus) {
                lastPauseStatus = false;
                agent.isStopped = false;
            }

            // Player is currently moving
            if (targetIndicator != null) {
                // Checks if player has reached the destination
                if (agent.transform.position.x == agent.destination.x && agent.transform.position.z == agent.destination.z) {
                    GameObject.Destroy(targetIndicator);
                    targetIndicator = null;
                    lineRenderer.positionCount = 0;
                } else {
                    lineRenderer.positionCount = agent.path.corners.Length;
                    lineRenderer.SetPositions(agent.path.corners);
                }
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100)) {
                Action action = new Action("move", this, clickPosition.point);
                eventManager.QueueAction(action);
                Vector3 placement = new Vector3(clickPosition.point.x, 0.5f, clickPosition.point.z);
                if (targetIndicator != null) {
                    GameObject.Destroy(targetIndicator);
                    lineRenderer.positionCount = 0;
                }
                targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(clickPosition.point, path);
                lineRenderer.positionCount = path.corners.Length;
                lineRenderer.SetPositions(path.corners);
            }
        }
    }

    private void DoMovementAction(Vector3 destination) {
        agent.destination = destination;
        Vector3 placement = new Vector3(destination.x, 0.5f, destination.z);
        if (targetIndicator != null) {
            GameObject.Destroy(targetIndicator);
            lineRenderer.positionCount = 0;
        }
        targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
        lineRenderer.positionCount = agent.path.corners.Length;
        lineRenderer.SetPositions(agent.path.corners);
    }

    public void OnActionEvent(EventManager.Action action) {
        if (action.getName().Equals("move")) {
            DoMovementAction(action.getDestination());
        }
    }
}
