using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, Idamageable<DamageableProperties, Vector3>
{
    public float health;
    public float maxHealth;

    public Transform Player;

    public float attackDistance;

    public float attackTimerOrg;

    public float currentAttackTimer;

    public DamageableProperties[] ressistancesAndWeaknessMultipliers;

    public AttackHeight thisAttackHeight;

    public int hitParticleID;

    public string hitParticleName;

    public Animator thisAnim;

    public string attackAnimation;

    private void Update()
    {

        if (currentAttackTimer < attackTimerOrg)
        {
            currentAttackTimer += 1 * Time.deltaTime;
        }


        if (Vector3.Distance(Player.position, transform.position) < attackDistance && currentAttackTimer >= attackTimerOrg)
        {
            Attack();
        }
    }

    public void Damage(DamageableProperties dmgProperties, Vector3 hitFromPosition)
    {
        double damage;

        damage = dmgProperties.damageAmmount;

        for (int i = 0; i < ressistancesAndWeaknessMultipliers.Length; i++)
        {
            if (ressistancesAndWeaknessMultipliers[i].typeOfDamage == dmgProperties.typeOfDamage)
            {
                damage *= ressistancesAndWeaknessMultipliers[i].damageAmmount;
            }
        }

        health -= (float)damage;

        if (health <= 0)
        {

            //MainPlayerCombat._instance.enemies.Remove(this);
            //MainPlayerCombat._instance.StillInCombat();
            Destroy(gameObject);
        }

        transform.RotateAround(transform.position, transform.position - hitFromPosition, 5 * Time.deltaTime);
    }

    public void Attack()
    {
        thisAnim.Play(attackAnimation);
        currentAttackTimer = 0;
    }

    public AttackHeight GetPos()
    {
        return thisAttackHeight;
    }

    public int GetHitParticle()
    {
        return hitParticleID;
    }

    public string GetHitParticleName()
    {
        return hitParticleName;
    }
}
