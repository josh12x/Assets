using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BreakableItem : MonoBehaviour, IDamageable<float> {


    public float health;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Damage(float dmg)
    {
        health -= dmg;

        if (health <= 0)
        {
            Break();
        }
    }

    public void Break()
    {
        Destroy(gameObject);
    }
}

public interface IDamageable<T>
{
    void Damage(float damageTaken);
}