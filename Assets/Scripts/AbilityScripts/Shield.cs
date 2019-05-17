using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class Shield : Ability
{

    private GameObject shieldPrefab;

    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        shieldPrefab = Resources.Load("Shield") as GameObject;
    }

    public override void CastAbility(GameObject owner, GameObject target)
    {
        Debug.Log("Casting shield");
        Attack attack = new Attack("ability", owner, target, 0, this);
        
        attackManager.QueueAttack(attack);
        AbilityCasted();
    }

    public override void CastAbility(GameObject owner, List<GameObject> targets)
    {
        // Nothing
    }

    public override void DoAbilityEffect(GameObject owner, GameObject target)
    {
        target.GetComponent<AttackableGameObject>().SetStatus(new Status("shielded", 5.0f));
        GameObject shieldSymbol = Instantiate(
            shieldPrefab,
            target.transform.position + new Vector3(0.0f, 7.0f, 0.0f),
            transform.rotation,
            target.transform
            );

        shieldSymbol.transform.localScale = (new Vector3(1/target.transform.lossyScale.x, 1/target.transform.lossyScale.y, 1/target.transform.lossyScale.z));
    }
}
