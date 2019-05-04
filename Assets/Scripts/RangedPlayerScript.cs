using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;
using static AttackManager;

public class RangedPlayerScript : SelectableCharacter, ActionableGameObject, AttackableGameObject {

    
    private readonly float ATTACK_RANGE = 6f;

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
}
