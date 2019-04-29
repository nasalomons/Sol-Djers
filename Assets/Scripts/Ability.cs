using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Ability : MonoBehaviour {

    public int cooldown;

    private TimeManager timeManager;
    private AttackManager attackManager;

    private float lastAttackTime;

    void Start() {
        timeManager = TimeManager.Instance;
        attackManager = AttackManager.Instance;
        lastAttackTime = -cooldown;
    }

    public bool IsCastable() {
        return timeManager.getTimeSeconds() - lastAttackTime > cooldown;
    }

    // This function should be called after an ActionableGameObject is told it can use the ability by the EventManager. 
    // This function will send the ability information to the AttackManager.
    public abstract void DoAbility(GameObject owner, GameObject target);
}
