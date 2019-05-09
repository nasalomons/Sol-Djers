using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;
using static AttackManager;

public abstract class SelectableCharacter : MonoBehaviour, ActionableGameObject, AttackableGameObject
{
    bool selected;

    protected SelectManager selectManager;

    /* Dividing Line for Stuff from RangedPlayerScript */

    protected NavMeshAgent agent;
    protected CameraScript mainCameraScript;
    protected Camera mainCamera;

    protected GameObject targetIndicatorPrefab;
    protected GameObject targetIndicator;
    protected GameObject autoAttackPrefab;

    protected EventManager eventManager;
    protected PauseManager pauseManager;
    protected TimeManager timeManager;
    protected AttackManager attackManager;
    private CutsceneManager cutsceneManager;

    protected bool lastPauseStatus;

    protected Action currentAction;
    protected long lastAttackTime;

    protected HealthScript overhead;
    protected bool isDead;

    protected Ability[] abilityList;

    public LineRenderer lineRenderer;

    public Texture2D cursorAbility;
    protected int abilityToCast;

    protected Animator animator;

    public void Start()
    {
        selectManager = FindObjectOfType<SelectManager>();

        /* Dividing Line for Stuff from RangedPlayerScript */

        agent = GetComponent<NavMeshAgent>();
        mainCameraScript = Camera.main.GetComponent<CameraScript>();
        mainCamera = Camera.main;

        eventManager = EventManager.Instance;
        eventManager.Subscribe(this);
        pauseManager = PauseManager.Instance;
        timeManager = TimeManager.Instance;
        attackManager = AttackManager.Instance;
        attackManager.Subscribe(this);
        cutsceneManager = CutsceneManager.Instance;

        targetIndicatorPrefab = Resources.Load("TargetPoint") as GameObject;
        targetIndicator = null;
        autoAttackPrefab = Resources.Load("RangedAutoAttack") as GameObject;

        lastPauseStatus = false;
        lastAttackTime = 0;

        overhead = gameObject.GetComponent<HealthScript>();
        isDead = false;

        abilityList = GetComponents<Ability>();
        abilityToCast = -1;

        animator = gameObject.GetComponent<Animator>();
    }

    public void SetSelected(bool val)
    {
        selected = val;
    }

    public bool GetSelected()
    {
        return selected;
    }

    public GameObject GetTargetIndicator()
    {
        return targetIndicator;
    }

    public void SetTargetIndicator(GameObject newTargetIndicator)
    {
        targetIndicator = newTargetIndicator;
    }

    public LineRenderer GetLineRenderer()
    {
        return lineRenderer;
    }

    /************ Dividing Line for stuff from RangedPlayerScript **************/

    // Update is called once per frame
    void Update()
    {
        if (!cutsceneManager.CutsceneHappening()) {
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

            if (Input.GetMouseButtonUp(0)) {
                if (abilityToCast >= 0 && this.GetSelected()) {
                    bool click = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100);

                    if (click) {
                        Action action = null;
                        if (selectManager.GetNumSelected() == 1 && this.GetSelected()) {
                            if (clickPosition.transform.gameObject.tag == "Enemy") {
                                if (abilityList[abilityToCast].IsCastable()) {
                                    action = new Action("ability" + abilityToCast, this, clickPosition);
                                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                                } else {
                                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                                }
                            } else if (abilityToCast == 1 && clickPosition.transform.gameObject.tag == "Ally") {
                                if (abilityList[1].IsCastable()) {
                                    action = new Action("ability" + abilityToCast, this, clickPosition);
                                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                                } else {
                                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                                }
                            }
                        } else {
                            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                        }

                        abilityToCast = -1;
                        selectManager.SetAbilityReady(false);

                        eventManager.QueueAction(action);

                        if (pauseManager.IsPaused()) {
                            currentAction = action;
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
            } else if (Input.GetKeyDown(KeyCode.Q) && this.GetSelected()) {
                Cursor.SetCursor(cursorAbility, Vector2.zero, CursorMode.Auto);
                abilityToCast = 0;
                selectManager.SetAbilityReady(true);
            } else if (Input.GetKeyDown(KeyCode.W) && this.GetSelected()) {
                Cursor.SetCursor(cursorAbility, Vector2.zero, CursorMode.Auto);
                abilityToCast = 1;
                selectManager.SetAbilityReady(true);
            }

            showAction();
        }
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void showAction()
    {
        if (currentAction == null)
        {
            overhead.UpdateAction(HealthScript.CurrentAction.NONE);
        }
        else
        {
            if (currentAction.getName().Equals("move"))
            {
                Vector3 placement = new Vector3(currentAction.getDestination().point.x, 0.5f, currentAction.getDestination().point.z);
                if (targetIndicator != null)
                {
                    GameObject.Destroy(targetIndicator);
                    lineRenderer.positionCount = 0;
                }
                targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
                NavMeshPath path = new NavMeshPath();
                agent.CalculatePath(currentAction.getDestination().point, path);
                lineRenderer.positionCount = path.corners.Length;
                lineRenderer.SetPositions(path.corners);

                overhead.UpdateAction(HealthScript.CurrentAction.MOVE);
            }
            else if (currentAction.getName().Equals("autoattack"))
            {
                if (targetIndicator != null)
                {
                    GameObject.Destroy(targetIndicator);
                    targetIndicator = null;
                    lineRenderer.positionCount = 0;
                }
                overhead.UpdateAction(HealthScript.CurrentAction.ATTACK);
            }
        }
    }

    protected void DoMovementAction(Action action, Action nextAction)
    {
        animator.SetBool("IsMoving", true);
        currentAction = action;
        agent.destination = action.getDestination().point;
        Vector3 placement = new Vector3(action.getDestination().point.x, 0.5f, action.getDestination().point.z);
        if (targetIndicator != null)
        {
            GameObject.Destroy(targetIndicator);
            lineRenderer.positionCount = 0;
        }
        targetIndicator = Instantiate(targetIndicatorPrefab, placement, Quaternion.identity);
        lineRenderer.positionCount = agent.path.corners.Length;
        lineRenderer.SetPositions(agent.path.corners);

        if (nextAction != null)
        {
            eventManager.QueueAction(nextAction);
        }
    }

    protected abstract void DoAttackAction(Action action);

    public void OnActionEvent(Action action)
    {
        if (action.getName().Equals("move"))
        {
            DoMovementAction(action, action.getNextAction());
        }
        else if (action.getName().Equals("autoattack"))
        {
            DoAttackAction(action);
        }
        else if (action.getName().Equals("ability0"))
        {
            GameObject target = action.getDestination().transform.gameObject;
            if (target != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsCastingAbility");
                abilityList[0].CastAbility(gameObject, target);
            }
        }
        else if (action.getName().Equals("ability1"))
        {
            GameObject target = action.getDestination().transform.gameObject;
            if (target != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsCastingAbility");
                abilityList[1].CastAbility(gameObject, target);
            }
        }
        showAction();
    }

    public void OnAttacked(AttackManager.Attack attack)
    {
        if (attack.getTarget().Equals(gameObject))
        {
            if (overhead.TakeDamage(attack.getDamage()))
            {
                // alive
                if (attack.getAbility() != null)
                {
                    attack.getAbility().DoAbilityEffect(gameObject);
                }
            }
            else
            {
                // dead
                isDead = true;
            }
        }
    }
}
