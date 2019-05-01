using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

/* This ability sends a damaging projectile at a target. When it hits the target, it
   creates two new projectiles that move perpendicular to the original projectile
   that do half damage each if they hit another enemy. */
public class SplittingRayAbility : Ability {

    private GameObject projectilePrefab;
    private float damage;

    new void Start() {
        base.Start();
        projectilePrefab = Resources.Load("SplittingRayProjectile") as GameObject;
        damage = 40;
    }
    
    public override void CastAbility(GameObject owner, GameObject target) {
        Attack attack = new Attack("ability", owner, target, damage, this);
        owner.transform.LookAt(target.transform);
        GameObject rayProjectile = Instantiate(
            projectilePrefab,
            transform.position + transform.forward * 1.5f,
            transform.rotation);
        rayProjectile.GetComponent<SplittingRayProjectileScript>().setAttack(attack);
        AbilityCasted();
    }

    public override void DoAbilityEffect(GameObject target) {
        // nothing
    }

    
}
