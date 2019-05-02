using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class Heal : Ability {
    new void Start() {
        base.Start();
    }

    public override void CastAbility(GameObject owner, GameObject target) {
        Attack attack = new Attack("ability", owner, target, -10);
        attackManager.QueueAttack(attack);
        AbilityCasted();
    }

    public override void DoAbilityEffect(GameObject target) {
        throw new System.NotImplementedException();
    }
}
