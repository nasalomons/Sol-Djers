using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public class ProjectileScript : MonoBehaviour {
    private Attack attack;
    private AttackManager attackManager;
    private PauseManager pauseManager;

    private void Start() {
        attackManager = AttackManager.Instance;
        pauseManager = PauseManager.Instance;
    }

    void FixedUpdate() {
        if (attack != null && !pauseManager.IsPaused()) {
            GameObject target = attack.getTarget();
            transform.position = Vector3.Lerp(transform.position, target.transform.position, Time.deltaTime * 10);
            transform.LookAt(target.transform);

            if ((transform.position - target.transform.position).magnitude < 0.3) {
                attackManager.QueueAttack(attack);
                Destroy(gameObject);
            }
        }
    }

    public void setAttack(Attack attack) {
        this.attack = attack;
    }
}
