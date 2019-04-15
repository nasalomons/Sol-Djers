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
    private Renderer rend;
    private bool selected;
    private Action currentAction;

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
        rend = transform.gameObject.GetComponent<Renderer>();
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
                    currentAction = null;
                } else {
                    lineRenderer.positionCount = agent.path.corners.Length;
                    lineRenderer.SetPositions(agent.path.corners);
                }
            }
        }

        if (Input.GetMouseButtonDown(0)) {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100)) {
                if (clickPosition.transform.gameObject.tag == "Ally")
                {
                    Debug.Log("Hit the Player!");
                    selected = true;
                    rend.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
                    
                }
                else
                {
                    selected = false;
                    rend.material.shader = Shader.Find("Diffuse");
                    if (targetIndicator != null) {
                        GameObject.Destroy(targetIndicator);
                        targetIndicator = null;
                        lineRenderer.positionCount = 0;
                    }
                }
            }
        }
        else if (Input.GetMouseButton(1) && selected)
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100))
            {
                Action action = new Action("move", this, clickPosition.point);
                eventManager.QueueAction(action);
                currentAction = action;
            }
        }

        if (selected) {
            showAction();
        }
    }

    private void showAction() {
        if (currentAction == null) {
            return;
        }

        if (currentAction.getName().Equals("move")) {
            Vector3 placement = new Vector3(currentAction.getDestination().x, 0.5f, currentAction.getDestination().z);
            if (targetIndicator != null) {
                GameObject.Destroy(targetIndicator);
                lineRenderer.positionCount = 0;
            }
            targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(currentAction.getDestination(), path);
            lineRenderer.positionCount = path.corners.Length;
            lineRenderer.SetPositions(path.corners);
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
