using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static EventManager;
using static AttackManager;

public class RangedPlayerScript : SelectableCharacter, ActionableGameObject, AttackableGameObject {

    
    private readonly float ATTACK_RANGE = 6f;

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
}
