using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour {

    public int cooldown;

    private TimeManager timeManager;
    protected AttackManager attackManager;

    private float lastAttackTime;

    protected void Start() {
        timeManager = TimeManager.Instance;
        attackManager = AttackManager.Instance;
        lastAttackTime = -cooldown;
    }

    public bool IsCastable() {
        return timeManager.getTimeSeconds() - lastAttackTime >= cooldown;
    }

    public void AbilityCasted() {
        lastAttackTime = timeManager.getTimeSeconds();
    }

    public int Cooldown() {
        return Mathf.Max(0, Mathf.RoundToInt(cooldown - (timeManager.getTimeSeconds() - lastAttackTime)));
    }

    // This function should be called after an ActionableGameObject is told it can use the ability by the EventManager. 
    // This function will send the ability information to the AttackManager.
    // Call AbilityCasted() at the end.
    public abstract void CastAbility(GameObject owner, GameObject target);

    // This function should be called by the affected target of an ability
    public abstract void DoAbilityEffect(GameObject target);
}
