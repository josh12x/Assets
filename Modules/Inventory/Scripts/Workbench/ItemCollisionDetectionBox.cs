using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollisionDetectionBox : MonoBehaviour
{
    public List<GameObject> gameObjectsWithinBox = new List<GameObject>();
    public List<Item> itemsWithinBox = new List<Item>();
    public List<Container> containersNearby = new List<Container>();
    public Workbench collisionBoxToThisWorkbench;


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<SaveableItem>() != null)
        {
            if (other.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.DroppedItem)
            {
                AddItemToPhysicalList(other.gameObject);
            }

            if (other.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container)
            {
                AddContainerToList(other.gameObject);
            }
        }
    }

    public void AddItemToPhysicalList(GameObject objectToAdd)
    {
        if (objectToAdd.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.DroppedItem)
        {
            if (!itemsWithinBox.Contains(objectToAdd.GetComponent<AddOnClick>().thisItem))
            {
                itemsWithinBox.Add(objectToAdd.GetComponent<AddOnClick>().thisItem);
                gameObjectsWithinBox.Add(objectToAdd);
                objectToAdd.GetComponent<AddOnClick>().insideCollisionBoxList = true;
                objectToAdd.GetComponent<AddOnClick>().collisionList = itemsWithinBox;
            }
        }
    }

    public void AddContainerToList(GameObject objectToAdd)
    {
        if (objectToAdd.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container)
        {
            if (!containersNearby.Contains(objectToAdd.GetComponent<Container>()))
            {
                containersNearby.Add(objectToAdd.GetComponent<Container>());
                objectToAdd.GetComponent<Container>().activeBench = collisionBoxToThisWorkbench;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<AddOnClick>() != null && other.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.DroppedItem)
        {
            if (itemsWithinBox.Contains(other.GetComponent<AddOnClick>().thisItem))
            {
                itemsWithinBox.Remove(other.GetComponent<AddOnClick>().thisItem);
                gameObjectsWithinBox.Remove(other.gameObject);
            }
        }
    }
}
