using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    public GameObject MonstertoSpawn;
    public Transform spawnPoint;

    public float spawnDelays;

	// Use this for initialization
	void Start ()
    {
        InvokeRepeating("Spawn", 1, spawnDelays);
	}


    void Spawn()
    {
        GameObject clone;
        clone = Instantiate(MonstertoSpawn, spawnPoint.position, spawnPoint.rotation);
    }
	// Update is called once per frame
	void Update () {
		
	}
}
