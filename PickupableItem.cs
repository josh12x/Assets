using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupableItem : MonoBehaviour
{

    public int itemAmount;
    public int itemID;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.CompareTag("Player"))
        {
            Additem();
        }
    }

    private void Additem()
    {
        Inventory.AddItemAnywhere(ItemDatabase.FetchByID(itemID), itemAmount);
        Destroy(gameObject);
    }
}
