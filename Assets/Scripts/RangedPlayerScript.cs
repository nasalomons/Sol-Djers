﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;
using static AttackManager;

public class RangedPlayerScript : SelectableCharacter, ActionableGameObject, AttackableGameObject {

    private readonly float ATTACK_RANGE = 6f;

    private NavMeshAgent agent;
    private CameraScript mainCameraScript;
    private Camera mainCamera;

    private GameObject targetIndicatorPrefab;
    private GameObject targetIndicator;
    private GameObject autoAttackPrefab;

    private EventManager eventManager;
    private PauseManager pauseManager;
    private TimeManager timeManager;
    private AttackManager attackManager;

    private bool lastPauseStatus;
    private Renderer rend;

    private Action currentAction;
    private long lastAttackTime;

    private HealthScript overhead;
    private bool isDead;

    private Ability[] abilityList;

    public LineRenderer lineRenderer;

    public Texture2D cursorAbility;
    private int abilityToCast;

    private Vector3 boxStartPosition;
    private Vector3 boxEndPosition;
    public Texture selectTexture;
    private SelectableCharacter[] selectableChars;

    private Animator animator;

    // Start is called before the first frame update
    void Start() {
        agent = GetComponent<NavMeshAgent>();
        mainCameraScript = Camera.main.GetComponent<CameraScript>();
        mainCamera = Camera.main;

        targetIndicatorPrefab = Resources.Load("TargetPoint") as GameObject;
        targetIndicator = null;
        autoAttackPrefab = Resources.Load("RangedAutoAttack") as GameObject;

        eventManager = EventManager.Instance;
        eventManager.Subscribe(this);
        pauseManager = PauseManager.Instance;
        timeManager = TimeManager.Instance;
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);

        lastPauseStatus = false;
        rend = transform.gameObject.GetComponentInChildren<Renderer>();
        lastAttackTime = 0;

        overhead = gameObject.GetComponent<HealthScript>();
        isDead = false;

        abilityList = GetComponents<Ability>();
        abilityToCast = -1;

        animator = gameObject.GetComponent<Animator>();
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
                animator.enabled = false;
            }

        } else {
            // If we aren't paused, but this still is
            if (lastPauseStatus) {
                lastPauseStatus = false;
                agent.isStopped = false;
                animator.enabled = true;
            }

            // Player is currently moving
            if (targetIndicator != null) {
                // Checks if player has reached the destination
                if (agent.transform.position.x == agent.destination.x && agent.transform.position.z == agent.destination.z) {
                    GameObject.Destroy(targetIndicator);
                    targetIndicator = null;
                    lineRenderer.positionCount = 0;
                    currentAction = null;
                    showAction();
                } else {
                    lineRenderer.positionCount = agent.path.corners.Length;
                    lineRenderer.SetPositions(agent.path.corners);
                }
            }

            if (agent.transform.position.x == agent.destination.x && agent.transform.position.z == agent.destination.z) {
                animator.SetBool("IsMoving", false);
            }
        }

        // Called while the user is holding the mouse down and we aren't trying to cast an ability.
        if (abilityToCast == -1) {
            if (Input.GetMouseButton(0)) {
                // Called on the first update where the user has pressed the mouse button.
                if (Input.GetMouseButtonDown(0))
                    boxStartPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
                else  // Else we must be in "drag" mode.
                    boxEndPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane);
            } else {
                // Handle the case where the player had been drawing a box but has now released.
                if (boxEndPosition != Vector3.zero && boxStartPosition != Vector3.zero)
                    HandleUnitSelection(boxStartPosition, boxEndPosition);
                // Reset box positions.
                boxEndPosition = boxStartPosition = Vector3.zero;
            }
        }

        if (GetSelected()) {
            mainCameraScript.setPlayer(gameObject);
            rend.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
        }

        if (Input.GetMouseButtonDown(0)) {
            if (abilityToCast >= 0 && this.GetSelected()) {
                bool click = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100);

                if (click) {
                    Action action = null;
                    if (clickPosition.transform.gameObject.tag == "Enemy") {
                        if (abilityList[abilityToCast].IsCastable()) {
                            action = new Action("ability" + abilityToCast, this, clickPosition);
                            abilityToCast = -1;
                            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        } else {
                            abilityToCast = -1;
                            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        }
                    } else if (abilityToCast == 1 && clickPosition.transform.gameObject.tag == "Ally") {
                        if (abilityList[1].IsCastable()) {
                            action = new Action("ability" + abilityToCast, this, clickPosition);
                            abilityToCast = -1;
                            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        } else {
                            abilityToCast = -1;
                            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        }
                    } else {
                        abilityToCast = -1;
                        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                    }

                    eventManager.QueueAction(action);

                    if (pauseManager.IsPaused()) {
                        currentAction = action;
                    }
                }
            } else if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100)) {
                if (clickPosition.transform.gameObject != gameObject) {
                    this.SetSelected(false);
                    rend.material.shader = Shader.Find("Diffuse");
                    if (targetIndicator != null) {
                        GameObject.Destroy(targetIndicator);
                        targetIndicator = null;
                        lineRenderer.positionCount = 0;
                    }
                }
            }
        } else if (Input.GetMouseButton(1) && this.GetSelected()) {
            bool click = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100);
            RaycastHit[] hits = Physics.RaycastAll(Camera.main.ScreenPointToRay(Input.mousePosition));

            if (click) {
                Action action = null;
                if (clickPosition.transform.gameObject.tag == "Enemy") {
                    action = new Action("autoattack", this, clickPosition);
                } else {                   
                    action = new Action("move", this, hits[hits.Length - 1]);
                }

                eventManager.QueueAction(action);

                if (pauseManager.IsPaused()) {
                    currentAction = action;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.Q)) {
            Cursor.SetCursor(cursorAbility, Vector2.zero, CursorMode.Auto);
            abilityToCast = 0;
        } else if (Input.GetKeyDown(KeyCode.W)) {
            Cursor.SetCursor(cursorAbility, Vector2.zero, CursorMode.Auto);
            abilityToCast = 1;
        } else if (Input.GetKeyUp(KeyCode.Alpha2)) {
            this.SetSelected(true);
            rend.material.shader = Shader.Find("Self-Illumin/Outlined Diffuse");
        } else if (Input.GetKeyDown(KeyCode.Alpha1)) {
            this.SetSelected(false);
            rend.material.shader = Shader.Find("Diffuse");
            if (targetIndicator != null) {
                GameObject.Destroy(targetIndicator);
                targetIndicator = null;
                lineRenderer.positionCount = 0;
            }
        }

        showAction();
    }

    public bool IsDead() {
        return isDead;
    }

    private void showAction() {
        if (currentAction == null) {
            overhead.UpdateAction(HealthScript.CurrentAction.NONE);
        } else {
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

                overhead.UpdateAction(HealthScript.CurrentAction.MOVE);
            } else if (currentAction.getName().Equals("autoattack")) {
                if (targetIndicator != null) {
                    GameObject.Destroy(targetIndicator);
                    targetIndicator = null;
                    lineRenderer.positionCount = 0;
                }
                overhead.UpdateAction(HealthScript.CurrentAction.ATTACK);
            } 
        }
    }

    private void DoMovementAction(Action action, Action nextAction) {
        animator.SetBool("IsMoving", true);
        currentAction = action;
        agent.destination = action.getDestination().point;
        Vector3 placement = new Vector3(action.getDestination().point.x, 0.5f, action.getDestination().point.z);
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
        Transform temp = action.getDestination().transform;
        if (temp == null) {
            return;
        }
        GameObject target = temp.gameObject;
        if (target.GetComponent<AttackableGameObject>().IsDead()) {
            return;
        }

        // if within attack range attack
        if ((transform.position - target.transform.position).magnitude <= ATTACK_RANGE) {
            currentAction = action;

            // stop moving
            agent.destination = transform.position;
            transform.LookAt(target.transform.position);

            // if we havent attacked in 2 seconds
            long currentTime = timeManager.getTimeSeconds();
            if (currentTime - lastAttackTime >= 2) {              
                Attack attack = new Attack("auto", gameObject, target, 10);
                GameObject autoAttack = Instantiate(autoAttackPrefab, transform.position + transform.forward * 1.5f, transform.rotation);
                autoAttack.GetComponent<RangedAutoAttackProjectile>().setAttack(attack);

                lastAttackTime = currentTime;

                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsAttacking");
            }

            // queue up next attack
            eventManager.QueueAction(action);

            // if not in range then move in range
        } else {
            Action newAction = new Action("move", this, action.getDestination(), action);
            eventManager.QueueAction(newAction);
        }
    }

    public void OnActionEvent(Action action) {
        if (action.getName().Equals("move")) {
            DoMovementAction(action, action.getNextAction());
        } else if (action.getName().Equals("autoattack")) {
            DoAttackAction(action);
        } else if (action.getName().Equals("ability0")) {
            GameObject target = action.getDestination().transform.gameObject;
            if (target != null) {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsCastingAbility");
                abilityList[0].CastAbility(gameObject, target);
            }            
        } else if (action.getName().Equals("ability1")) {
            GameObject target = action.getDestination().transform.gameObject;
            if (target != null) {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsCastingAbility");
                abilityList[1].CastAbility(gameObject, target);
            }
        }
        showAction();
    }

    public void OnAttacked(AttackManager.Attack attack) {
        if (attack.getTarget().Equals(gameObject)) {
            if (overhead.TakeDamage(attack.getDamage())) {
                // alive
                if (attack.getAbility() != null) {
                    attack.getAbility().DoAbilityEffect(gameObject);
                }
            } else {
                // dead
                isDead = true;
            }
        }
    }

    public void HandleUnitSelection(Vector3 boxStartPosition, Vector3 boxEndPosition)
    {
        
        selectableChars = FindObjectsOfType<SelectableCharacter>();
        Rect rect;
        float width = Mathf.Abs(boxStartPosition.x - boxEndPosition.x);
        float height = Mathf.Abs(boxStartPosition.y - boxEndPosition.y);
        if (boxStartPosition.x <= boxEndPosition.x) {
            if (boxStartPosition.y <= boxEndPosition.y) {
                rect = new Rect(boxStartPosition.x, boxStartPosition.y, width, height);
            } else {
                rect = new Rect(boxStartPosition.x, boxEndPosition.y, width, height);
            }
        } else {
            if (boxStartPosition.y <= boxEndPosition.y) {
                rect = new Rect(boxEndPosition.x, boxStartPosition.y, width, height);
            } else {
                rect = new Rect(boxEndPosition.x, boxEndPosition.y, width, height);
            }
        }
       
        foreach (SelectableCharacter selChar in selectableChars)
        {          
            Vector2 position = mainCamera.WorldToScreenPoint(selChar.gameObject.transform.position);
            Rect charRect = new Rect(position.x - 50, position.y, 100, 150);
            if (charRect.Overlaps(rect)) {
                selChar.SetSelected(true);
            }
        }
    }

    public void OnGUI()
    {
        
        // If we are in the middle of a selection draw the texture.
        if (boxStartPosition != Vector3.zero && boxEndPosition != Vector3.zero)
        {

            GUI.color = new Color(1.0f, 1.0f, 1.0f, 0.5f); //0.5 is half opacity

            // Create a rectangle object out of the start and end position while transforming it
            // to the screen's cordinates.
            var rect = new Rect(boxStartPosition.x, Screen.height - boxStartPosition.y,
                                boxEndPosition.x - boxStartPosition.x,
                                -1 * (boxEndPosition.y - boxStartPosition.y));
            // Draw the texture.
            GUI.DrawTexture(rect, selectTexture);
        } else
        {
            GUI.color = Color.white;
        }
    }
}
