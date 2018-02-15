using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrownDamagable : MonoBehaviour
{
    public float damageAmount;
    public Rigidbody2D thisBod;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log("Hit Something");
        if (col.GetComponent<IDamageable<float>>() != null)
        {
            Debug.Log("Hit a damagable");
            col.GetComponent<IDamageable<float>>().Damage(damageAmount);
        }

        thisBod.velocity = Vector2.zero;

        thisBod.Sleep();

        transform.SetParent(col.transform);
    }
}
