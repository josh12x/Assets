using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AddOnClick : SaveableItem
{
    //the base item class this dropped item refferences.
    [HideInInspector]
    public Item thisItem;


    //how many items are in this stack/dropped item
    public int Amnt;


    //do we create the item from our database at the start of the game
    public bool createItemAtStartFromDatabase;


    //need to get back to this
    public bool insideCollisionBoxList;


    //need to get back to this
    public List<Item> collisionList;


    //Id of the item that is dropped
    public int id;

    //the type of item that is dropped
    public int type;


    //the name of the item that is dropped
    public string itemName;


    //need to get back to this
    public bool DroppedItem = false;


    //the sound it makes when we pick this item up.
    public AudioClip pickupSound;


    public void Start()
    {
        DelayedStart();
    }


    public void DelayedStart()
    {

        if (DroppedItem == false)
        {
            if (createItemAtStartFromDatabase)
            {
                thisItem = Inventory._InvRef.TypeChecker(ItemDatabase.FetchByID(id));
                thisItem.StackSize = Amnt;
            }
        }
    }

}
