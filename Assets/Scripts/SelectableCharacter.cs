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
    private Vector3 pauseVelocity;

    protected Action currentAction;
    protected long lastAttackTime;

    protected HealthScript overhead;
    protected bool isDead;

    protected Ability[] abilityList;

    public LineRenderer lineRenderer;

    public Texture2D cursorAbility;
    protected int abilityToCast;

    protected Animator animator;

    private bool hotbarClicked;

    public void Start()
    {
        selectManager = SelectManager.Instance;

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

        hotbarClicked = false;
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

    public void HotbarClicked() {
        hotbarClicked = true;
    }

    /************ Dividing Line for stuff from RangedPlayerScript **************/

    // Update is called once per frame
    void Update() {
        if (!IsDead()) {
            if (!cutsceneManager.CutsceneHappening()) {
                if (pauseManager.IsPaused()) {
                    // If we are now paused, and this isn't paused yet
                    if (!lastPauseStatus) {
                        lastPauseStatus = true;
                        pauseVelocity = agent.velocity;
                        agent.velocity = Vector3.zero;
                        agent.isStopped = true;
                        animator.enabled = false;
                    }

                } else {
                    // If we aren't paused, but this still is
                    if (lastPauseStatus) {
                        lastPauseStatus = false;
                        agent.velocity = pauseVelocity;
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
                            ShowAction();
                        } else {
                            lineRenderer.positionCount = agent.path.corners.Length;
                            lineRenderer.SetPositions(agent.path.corners);
                        }
                    }

                    if (agent.transform.position.x == agent.destination.x && agent.transform.position.z == agent.destination.z) {
                        animator.SetBool("IsMoving", false);
                    }
                }

                if (Input.GetMouseButtonDown(0)) {
                    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
                }

                if (Input.GetMouseButtonUp(0)) {                   
                    if (hotbarClicked) {
                        hotbarClicked = false;
                    } else {
                        if (abilityToCast >= 0 && this.GetSelected()) {
                            bool click = Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit clickPosition, 100);

                            if (click) {
                                Action action = null;
                                if (selectManager.GetNumSelected() == 1 && this.GetSelected()) {
                                    if (clickPosition.transform.gameObject.tag == "Enemy") {
                                        action = new Action("ability" + abilityToCast, this, clickPosition);
                                    } else if (abilityToCast == 1 && clickPosition.transform.gameObject.tag == "Ally") {
                                        action = new Action("ability" + abilityToCast, this, clickPosition);
                                    }
                                }

                                abilityToCast = -1;
                                selectManager.SetAbilityReady(false);

                                eventManager.QueueAction(action);

                                if (pauseManager.IsPaused()) {
                                    currentAction = action;
                                }
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
                    PrepareAbility(0);
                } else if (Input.GetKeyDown(KeyCode.W) && this.GetSelected()) {
                    PrepareAbility(1);
                } else if (Input.GetKeyDown(KeyCode.E) && this.GetSelected()) {
                    PrepareAbility(2);
                }

                ShowAction();
            }
        }
    }

    public abstract void PrepareAbility(int abilityIndex);

    public bool IsDead()
    {
        return isDead;
    }

    public Ability[] GetAbilities() {
        return abilityList;
    }

    public void ShowAction()
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

    public abstract void OnActionEvent(Action action);

    public void OnAttacked(AttackManager.Attack attack)
    {
        if (attack.GetTarget().Equals(gameObject))
        {
            if (overhead.TakeDamage(attack.GetDamage()))
            {
                // alive
                if (attack.GetAbility() != null)
                {
                    attack.GetAbility().DoAbilityEffect(attack.GetOwner(), gameObject);
                }
            }
            else
            {
                // dead
                isDead = true;
                animator.SetTrigger("IsDead");
                SetSelected(false);
                gameObject.GetComponentInChildren<Renderer>().material.shader = Shader.Find("Diffuse");
                attackManager.Unsubscribe(this);
                eventManager.Unsubscribe(this);
                Destroy(this);
            }
        }
    }

    public void SetStatus(Status status) {
        // will never happen
    }
}
