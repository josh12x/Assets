using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Harvestable : SaveableItem, Idamageable<DamageableProperties, Vector3>
{
    public float health;

    public GameObject harvestedPrefab;

    public bool shake;

    public DamageableProperties[] ressistancesAndWeaknessMultipliers;

    public AttackHeight thisAttackHeight;

    public int hitParticleID;
    public string hitParticleName;


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
            Harvest();
        }

        transform.RotateAround(transform.position, transform.position - hitFromPosition, 5 * Time.deltaTime);
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

    public void Harvest()
    {
        Invoke("SpawnHarvestedResult", .8f);
        Destroy(gameObject, .8f);
    }

    public void SpawnHarvestedResult()
    {
        GameObject clone;
        clone = Instantiate(harvestedPrefab, transform.position, transform.rotation) as GameObject;
        clone.transform.localScale = harvestedPrefab.transform.localScale;
    }
}
