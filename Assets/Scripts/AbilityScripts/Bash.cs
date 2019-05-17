using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class Bash : Ability
{

    private GameObject stunPrefab;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        stunPrefab = Resources.Load("BeveledStar") as GameObject;
    }

    public override void CastAbility(GameObject owner, GameObject target)
    {
        Attack attack = new Attack("ability", owner, target, 3.0f, this);
        attackManager.QueueAttack(attack);
        AbilityCasted();
    }

    public override void CastAbility(GameObject owner, List<GameObject> targets)
    {
        // Nothing
    }

    public override void DoAbilityEffect(GameObject owner, GameObject target)
    {
        target.GetComponent<AttackableGameObject>().SetStatus(new Status("stunned", 5.0f));

        GameObject stunSymbol = Instantiate(
            stunPrefab,
            target.transform.position + new Vector3(0.0f, 7.0f, 0.0f),
            transform.rotation,
            target.transform
            );

        stunSymbol.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
    }

}
