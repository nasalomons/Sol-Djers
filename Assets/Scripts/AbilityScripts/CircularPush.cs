using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class CircularPush : Ability { 
    // Start is called before the first frame update
    new void Start() {
        base.Start(); 
    }
    
    public override void CastAbility(GameObject owner, GameObject target) {
        Attack attack = new Attack("ability", owner, target, 0, this);
        AbilityCasted();
    }

    public override void CastAbility(GameObject owner, List<GameObject> targets) {
        foreach (GameObject target in targets) {
            Attack attack = new Attack("ability", owner, target, 0, this);
            attackManager.QueueAttack(attack);
        }
        AbilityCasted();
    }

    public override void DoAbilityEffect(GameObject owner, GameObject target) {            
        target.GetComponent<AttackableGameObject>().SetStatus(new Status("push", owner.transform.position));
    }
}
