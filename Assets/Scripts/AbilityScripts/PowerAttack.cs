using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class PowerAttack : Ability
{

    public GameObject blast;

    private GameObject temp;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        temp = null;
    }

    public override void CastAbility(GameObject owner, GameObject target)
    {
        Attack attack = new Attack("ability", owner, target, 10.0f, this);
        attackManager.QueueAttack(attack);
        AbilityCasted();
    }

    public override void CastAbility(GameObject owner, List<GameObject> targets)
    {
        // Nothing
    }

    public override void DoAbilityEffect(GameObject owner, GameObject target)
    {
        temp = Instantiate(blast, target.transform.position, Quaternion.Euler(new Vector3(270, 0, 0)), target.transform);
        temp.transform.localScale = new Vector3(8.0f, 8.0f, 8.0f);
        Invoke("StopParticles", 0.8f);
    }

    private void StopParticles()
    {
        Destroy(temp);
        temp = null;
    }

}
