using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour {

    public Rigidbody2D thisBod;
    public float speed;
    public Transform targetReticle;
	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        thisBod.velocity = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")) * speed * Time.deltaTime;

        //Quaternion targetRotation = Quaternion.LookRotation(new Vector3(targetReticle.position.x, targetReticle.position.y, transform.position.z), transform.up);
       // transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime);

        transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, Mathf.Atan2(-Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical")) * Mathf.Rad2Deg);
    }
}
