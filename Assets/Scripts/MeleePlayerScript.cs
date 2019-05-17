using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;
using static AttackManager;

public class MeleePlayerScript : SelectableCharacter, ActionableGameObject, AttackableGameObject {

    private readonly float ATTACK_RANGE = 4;    

    protected override void DoAttackAction(Action action) {
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
            agent.velocity = Vector3.zero;
            agent.destination = transform.position;
            transform.LookAt(new Vector3(target.transform.position.x, transform.position.y, target.transform.position.z));

            // if we havent attacked in 2 seconds
            long currentTime = timeManager.getTimeSeconds();
            if (currentTime - lastAttackTime >= 2) {
                Attack attack = new Attack("auto", gameObject, target, 10);
                attackManager.QueueAttack(attack);

                lastAttackTime = currentTime;

                animator.SetTrigger("IsAttacking");
                animator.SetBool("IsMoving", false);
            }

            // queue up next attack
            eventManager.QueueAction(action);

        // if not in range then move in range
        } else {
            Action newAction = new Action("move", this, action.getDestination(), action);
            eventManager.QueueAction(newAction);
        }
    }

    public override void PrepareAbility(int abilityIndex) {
        Debug.Log("Preparing ability " + abilityIndex.ToString());
        if (abilityList[abilityIndex].IsCastable()) {
            Debug.Log("Ability is castable");
            Cursor.SetCursor(cursorAbility, Vector2.zero, CursorMode.Auto);
            abilityToCast = abilityIndex;
            selectManager.SetAbilityReady(true);
        }
    }

    public override void OnActionEvent(Action action) {
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
        } else if (action.getName().Equals("ability2")) {
            GameObject target = action.getDestination().transform.gameObject;
            if (target != null)
            {
                animator.SetBool("IsMoving", false);
                animator.SetTrigger("IsCastingAbility");
                abilityList[2].CastAbility(gameObject, target);
            }
        }


        ShowAction();
    }
}
