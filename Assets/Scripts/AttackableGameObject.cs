using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static AttackManager;

public interface AttackableGameObject {
    void OnAttacked(Attack attack);
    bool IsDead();
}
