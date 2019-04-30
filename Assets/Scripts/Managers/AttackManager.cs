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

        public string getName() {
            return name;
        }
        public GameObject getOwner() {
            return owner;
        }
        public GameObject getTarget() {
            return target;
        }
        public float getDamage() {
            return damage;
        }
        public Ability getAbility() {
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
        List<int> indexList = new List<int>();
        foreach (Attack queued in attackQueue) {
            if (queued.getOwner().Equals(attack.getOwner())) {
                indexList.Add(attackQueue.IndexOf(queued));
            }
        }
        foreach (int index in indexList) {
            attackQueue.RemoveAt(index);
        }

        // Add attack to the end of the queue
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
