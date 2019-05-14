using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Sends out attacks to anything that can be attacked. It is up to the AttackableGameObjects to determine if they are the target of the attack. */
public sealed class AttackManager : MonoBehaviour {

    public class Attack {
        private string name;
        private GameObject owner;
        private GameObject target;
        private List<GameObject> targets;
        private float damage;
        private Ability ability;

        public Attack(string name, GameObject owner, GameObject target, float damage) {
            this.name = name;
            this.owner = owner;
            this.target = target;
            this.damage = damage;
        }

        public Attack(string name, GameObject owner, GameObject target, float damage, Ability ability) : this(name, owner, target, damage) {
            this.ability = ability;
        }

        public Attack(string name, GameObject owner, List<GameObject> targets, float damage, Ability ability) {
            this.name = name;
            this.owner = owner;
            this.targets = targets;
            this.damage = damage;
            this.ability = ability;
        }

        public string GetName() {
            return name;
        }
        public GameObject GetOwner() {
            return owner;
        }
        public GameObject GetTarget() {
            return target;
        }
        public List<GameObject> GetTargets() {
            return targets;
        }
        public float GetDamage() {
            return damage;
        }
        public Ability GetAbility() {
            return ability;
        }
    }

    // Creating event with parameter
    private class AttackEvent : UnityEvent<Attack> {
    }

    // Singleton of this event manager
    private static AttackManager attackManager;
    // Queue of actions to send out
    private List<Attack> attackQueue;
    private AttackEvent attackEvent;
    private PauseManager pauseManager;

    public static AttackManager Instance {
        get {
            if (attackManager == null) {
                attackManager = FindObjectOfType(typeof(AttackManager)) as AttackManager;
                attackManager.SetUp();
            }

            return attackManager;
        }
    }

    private void SetUp() {
        attackQueue = new List<Attack>();
        attackEvent = new AttackEvent();
        pauseManager = PauseManager.Instance;
    }

    public void QueueAttack(Attack attack) {
        attackQueue.Add(attack);
    }

    public void Subscribe(AttackableGameObject owner) {
        UnityAction<Attack> callback = owner.OnAttacked;
        attackEvent.AddListener(callback);
    }

    public void Unsubscribe(AttackableGameObject owner) {
        UnityAction<Attack> callback = owner.OnAttacked;
        attackEvent.RemoveListener(callback);
    }

    void Update() {
        if (!pauseManager.IsPaused() && attackQueue.Count > 0) {
            Attack attack = attackQueue[0];
            attackQueue.Remove(attack);
            attackEvent.Invoke(attack);
        }
    }
}
