using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Idamageable<T, X>
{
    void Damage(T dmgProperty, X hitFrom);

    AttackHeight GetPos();

    int GetHitParticle();

    string GetHitParticleName();
}


