﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;
using static AttackManager;

public class PlayerScript : MonoBehaviour, ActionableGameObject, AttackableGameObject {

    private readonly float ATTACK_RANGE = 2.5f;
    
    private NavMeshAgent agent;

    private GameObject targetIndicatorPrefab;
    private GameObject targetIndicator;

    private EventManager eventManager;
    private PauseManager pauseManager;
    private TimeManager timeManager;
    private AttackManager attackManager;

    private bool lastPauseStatus;
    private Renderer rend;
    private bool selected;

    private Action currentAction;
    private long lastAttackTime;

    private float currentHealth;

    public LineRenderer lineRenderer;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        targetIndicatorPrefab = Resources.Load("TargetPoint") as GameObject;
        targetIndicator = null;

        eventManager = EventManager.Instance;
        eventManager.Subscribe(this);
        pauseManager = PauseManager.Instance;
        timeManager = TimeManager.Instance;
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);

        lastPauseStatus = false;
        rend = transform.gameObject.GetComponent<Renderer>();
        lastAttackTime = 0;

        currentHealth = 100;
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
                if (clickPosition.transform.gameObject.tag == "Ally") {
                    Debug.Log("Hit the Player!");
                    selected = true;
                    rend.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");

                } else {
                    selected = false;
                    rend.material.shader = Shader.Find("Diffuse");
                    if (targetIndicator != null) {
                        GameObject.Destroy(targetIndicator);
                        targetIndicator = null;
                        lineRenderer.positionCount = 0;
                    }
                }
            }
        } else if (Input.GetMouseButton(1) && selected) {
            bool click = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100);

            if (click) {
                Action action;
                if (clickPosition.transform.gameObject.tag == "Enemy") {
                    action = new Action("autoattack", this, clickPosition);                    
                } else {
                    action = new Action("move", this, clickPosition);                
                }

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
            Vector3 placement = new Vector3(currentAction.getDestination().point.x, 0.5f, currentAction.getDestination().point.z);
            if (targetIndicator != null) {
                GameObject.Destroy(targetIndicator);
                lineRenderer.positionCount = 0;
            }
            targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
            NavMeshPath path = new NavMeshPath();
            agent.CalculatePath(currentAction.getDestination().point, path);
            lineRenderer.positionCount = path.corners.Length;
            lineRenderer.SetPositions(path.corners);
        }
    }

    private void DoMovementAction(Vector3 destination, Action nextAction) {
        agent.destination = destination;
        Vector3 placement = new Vector3(destination.x, 0.5f, destination.z);
        if (targetIndicator != null) {
            GameObject.Destroy(targetIndicator);
            lineRenderer.positionCount = 0;
        }
        targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
        lineRenderer.positionCount = agent.path.corners.Length;
        lineRenderer.SetPositions(agent.path.corners);

        if (nextAction != null) {
            eventManager.QueueAction(nextAction);
        }
    }

    private void DoAttackAction(Action action) {
        GameObject targetObject = action.getDestination().transform.gameObject;
        if (targetObject == null) {
            return;
        }

        AttackableGameObject target = targetObject.GetComponent<AttackableGameObject>();
        if (target == null) {
            return;
        }

        // if within attack range attack
        if ((transform.position - targetObject.transform.position).magnitude <= ATTACK_RANGE) {
            // stop moving
            agent.destination = transform.position;

            // if we havent attacked in 2 seconds
            long currentTime = timeManager.getTimeSeconds();
            if (currentTime - lastAttackTime >= 2) {
                Debug.Log("attack at time " + currentTime);

                Attack attack = new Attack("auto", target, 10);
                attackManager.QueueAttack(attack);

                lastAttackTime = currentTime;
            }

            // queue up next attack
            eventManager.QueueAction(action);

        // if not in range then move in range
        } else {
            Action newAction = new Action("move", this, action.getDestination(), action);
            eventManager.QueueAction(newAction);
        }
    }

    public void OnActionEvent(EventManager.Action action) {
        if (action.getName().Equals("move")) {
            DoMovementAction(action.getDestination().point, action.getNextAction());
        } else if (action.getName().Equals("autoattack")) {
            DoAttackAction(action);
        }
    }

    public void OnAttacked(AttackManager.Attack attack) {
        currentHealth -= attack.getDamage();
    }
}