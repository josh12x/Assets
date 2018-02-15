using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slime : MonoBehaviour {

    public SpriteRenderer mainSprite;

    public Sprite frontSprite;

    public Sprite backSprite;

    public bool movingDown;

	// Use this for initialization
	void Start () {
        InvokeRepeating("Move", 5, 5);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Move()
    {
        movingDown = !movingDown;

        if (movingDown)
        {
            mainSprite.sprite = frontSprite;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, -1);
        }
        else
        {
            mainSprite.sprite = backSprite;
            GetComponent<Rigidbody2D>().velocity = new Vector2(0, 1);
        }
    }
}
