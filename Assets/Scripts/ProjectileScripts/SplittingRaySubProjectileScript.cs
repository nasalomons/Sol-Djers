using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class SplittingRaySubProjectileScript : MonoBehaviour {
    private GameObject originTarget;
    private AttackManager attackManager;
    private PauseManager pauseManager;
    private TimeManager timeManager;
    private float timeCreated;

    private float damage;

    void Start() {
        attackManager = AttackManager.Instance;
        pauseManager = PauseManager.Instance;
        timeManager = TimeManager.Instance;
        damage = 20;
        timeCreated = timeManager.getTimeSeconds();
    }

    void FixedUpdate() {
        if (!pauseManager.IsPaused()) {
            if (timeManager.getTimeSeconds() - timeCreated > 5) {
                Destroy(gameObject);
            } else {
                transform.position += transform.forward * Time.deltaTime * 7;
            }
        } 
    }

    private void OnTriggerEnter(Collider other) {
        AttackableGameObject target = other.GetComponent<AttackableGameObject>();
        if (target != null && !other.gameObject.Equals(originTarget)) {
            Attack attack = new Attack("ability", null, other.gameObject, damage);
            attackManager.QueueAttack(attack);
            Destroy(gameObject);
        } 
    }

    public void setOriginTarget(GameObject target) {
        originTarget = target;
    }
}
