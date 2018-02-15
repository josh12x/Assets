using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackableItem : MonoBehaviour
{

    public AttackType thisAttackType;

    public DamageType thisDamage;

    public float range;

    public EquipedSlot currentHandEquipedTo;

    public Animator characterAnimator;

    public Transform playerTransform;

    public LayerMask onlyDamageableMask;

    public Transform DebugSpotting;

    public GameObject thrownPrefab;

    public Transform throwPoint;

    public float thrownVelocity;

    public float attackRate;

    public float orgAttackRate;

    public bool hasKnockback;

    public float knockback;

    public void Update()
    {
        if (currentHandEquipedTo == EquipedSlot.RightArm)
        {
            if (Input.GetAxis("RightTrigger") == 1)
            {
                Attack();
            }
        }

        if (currentHandEquipedTo == EquipedSlot.LeftArm)
        {
            if (Input.GetAxis("LeftTrigger") == 1)
            {
                Attack();
            }
        }


        if (attackRate > 0)
        {
            attackRate -= 1 * Time.deltaTime;
        }
    }

    public void Attack()
    {
        if (attackRate <= 0)
        {
            if (currentHandEquipedTo == EquipedSlot.RightArm)
            {

                if (thisAttackType == AttackType.Sword)
                {
                    MeleeDamage();
                    characterAnimator.Play("RightSwordSwing");
                }
            }

            if (currentHandEquipedTo == EquipedSlot.LeftArm)
            {

                if (thisAttackType == AttackType.ThrownDagger)
                {
                    ThrowDamage();
                    characterAnimator.Play("LeftHandDaggerThrow");
                }
            }
                Debug.Log("Attacked");

            attackRate = orgAttackRate;
        }
    }

    public void ThrowDamage()
    {
        GameObject clone;
        clone = Instantiate(thrownPrefab, throwPoint.position, throwPoint.rotation);

        clone.GetComponent<Rigidbody2D>().velocity = (playerTransform.position - throwPoint.position).normalized * -thrownVelocity;
    }

    public void MeleeDamage()
    {
        RaycastHit2D[] hitColliders;
        hitColliders = Physics2D.CircleCastAll(playerTransform.position + new Vector3(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"), 0), range, playerTransform.forward, onlyDamageableMask);


        Debug.Log(hitColliders.Length);
        DebugSpotting.position = playerTransform.position + new Vector3(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"), 0);

        for (int i = 0; i < hitColliders.Length; i++)
        {
            if (hitColliders[i].collider.GetComponent<IDamageable<float>>() != null)
            {
                hitColliders[i].collider.GetComponent<IDamageable<float>>().Damage((float)thisDamage.primaryDamageAmounts[0]);

                if (hasKnockback)
                {
                    hitColliders[i].collider.GetComponent<Rigidbody2D>().AddForce(hitColliders[i].transform.position - (playerTransform.position - hitColliders[i].transform.position).normalized * knockback);
                }
            }
        }
        
    }
}

[System.Serializable]
public class DamageType
{
    public int primaryDamageCount;
    public PrimaryDamageType[] primaryDamageTypes;
    public double[] primaryDamageAmounts;

    public int additonalDamageCount;
    public AdditionalDamageType[] thisAdditonalDamageTypes;
    public double[] additonalDamageAmounts;

}

public enum EquipedSlot
{
    RightArm,
    LeftArm
}

public enum AttackType
{
    Sword,
    ThrownDagger,
    Ect
}

public enum PrimaryDamageType
{
    Melee,
    Ranged,
    Magic
}

public enum AdditionalDamageType
{
    Slash,
    Blunt,
    Fire,
    Acid,
    ect
}
