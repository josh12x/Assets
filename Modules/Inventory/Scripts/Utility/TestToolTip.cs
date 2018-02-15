using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TestToolTip : MonoBehaviour 
{
    public string newToolTip;
    public Text textToChange;
	// Update is called once per frame
	void Update () 
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            newToolTip = ("Bronze Sword" + "\n A Bronze Sword" + "\n Damage : 2" + "\n Value : 60");
            textToChange.text = newToolTip;
        }


	}
}
