using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedAutoAttackProjectile : TargetedProjectileScript {

    new void Start() {
        base.Start();
    }

    protected override void DoOtherEffects(AttackManager.Attack attack) {
        // None
    }
}
