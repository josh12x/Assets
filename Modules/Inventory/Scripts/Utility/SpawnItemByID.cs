using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SpawnItemByID : MonoBehaviour 
{
    public InputField ID;
    public InputField Ammount;

    public void SpawnItem()
    {
        Inventory.AddItemAnywhere(ItemDatabase.FetchByID(int.Parse(ID.text)), int.Parse(Ammount.text));
    }
}
