using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplittingRayProjectileScript : TargetedProjectileScript {
    private GameObject subProjectilePrefab;

    // Start is called before the first frame update
    new void Start() {
        base.Start();
        subProjectilePrefab = Resources.Load("SplittingRaySubProjectile") as GameObject;
    }

    // Update is called once per frame
    void Update() {
        
    }

    protected override void DoOtherEffects(AttackManager.Attack attack) {
        GameObject leftSubProjectile = Instantiate(
            subProjectilePrefab,
            transform.position,
            transform.rotation * Quaternion.Euler(0, 90, 0));
        leftSubProjectile.GetComponent<SplittingRaySubProjectileScript>().setOriginTarget(attack.GetTarget());
        GameObject rightSubProjectile = Instantiate(
            subProjectilePrefab,
            transform.position,
            transform.rotation * Quaternion.Euler(0, 270, 0));
        rightSubProjectile.GetComponent<SplittingRaySubProjectileScript>().setOriginTarget(attack.GetTarget());
    }
}
