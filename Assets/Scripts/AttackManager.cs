using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/* Sends out attacks to anything that can be attacked. It is up to the AttackableGameObjects to determine if they are the target of the attack. */
public sealed class AttackManager : MonoBehaviour {

    public class Attack {
        private string name;
        private AttackableGameObject target;
        private float damage;
        private Object effectData;

        public Attack(string name, AttackableGameObject target, float damage) {
            this.name = name;
            this.target = target;
            this.damage = damage;
        }

        public Attack(string name, AttackableGameObject target, float damage, Object effectData) : this(name, target, damage) {
            this.effectData = effectData;
        }

        public string getName() {
            return name;
        }
        public AttackableGameObject getTarget() {
            return target;
        }
        public float getDamage() {
            return damage;
        }
        public Object getEffectData() {
            return effectData;
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
