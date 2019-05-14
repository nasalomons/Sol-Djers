using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class Heal : Ability {
    public ParticleSystem ray;

    private ParticleSystem temp;

    new void Start() {
        base.Start();
        temp = null;
    }

    public override void CastAbility(GameObject owner, GameObject target) {
        Attack attack = new Attack("ability", owner, target, -10, this);
        attackManager.QueueAttack(attack);
        AbilityCasted();
    }

    public override void DoAbilityEffect(GameObject owner, GameObject target) {
        temp = Instantiate(ray, target.transform.position, Quaternion.Euler(new Vector3(270, 0, 0)));
        temp.Play();
        Invoke("StopParticles", 1);
    }

    private void StopParticles() {
        temp.Stop();
        temp = null;
    }

    public override void CastAbility(GameObject owner, List<GameObject> targets) {
        // nothing
    }
}
