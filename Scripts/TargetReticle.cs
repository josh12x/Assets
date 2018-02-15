using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetReticle : MonoBehaviour
{
    public Transform player;
    public float distanceMultiplier;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = player.position + (new Vector3(Input.GetAxis("RightStickHorizontal"), Input.GetAxis("RightStickVertical"), 0) * distanceMultiplier);
	}
}
