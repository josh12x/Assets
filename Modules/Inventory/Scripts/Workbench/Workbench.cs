using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ItemCollisionDetectionBox))]
public class Workbench : MonoBehaviour
{
    public ItemCollisionDetectionBox collisionBoxReference;

    public BoxCollider triggerZone;

    public Recipe output;

    public Transform workbenchOutputLocation;

    public bool playerNearby;

    public List<Item> ItemsToUse;

    List<ItemAndAmmountReference> ghostDestroyList = new List<ItemAndAmmountReference>();

    public Collider[] foundObjectsInTrigger;

    public LayerMask itemDetectionMask;

    private void Start()
    {
        ItemsToUse.Clear();

        collisionBoxReference.itemsWithinBox.Clear();

        collisionBoxReference.containersNearby.Clear();

        foundObjectsInTrigger = Physics.OverlapBox(triggerZone.bounds.center, triggerZone.bounds.extents, triggerZone.transform.rotation, itemDetectionMask);

        collisionBoxReference.gameObjectsWithinBox.RemoveAll(GameObject => GameObject == null);

        collisionBoxReference.containersNearby.RemoveAll(Container => Container == null);

        foundObjectsInTrigger = new Collider[0];

        for (int i = 0; i < foundObjectsInTrigger.Length; i++)
        {
            if (foundObjectsInTrigger[i].gameObject.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container)
            {
                collisionBoxReference.AddContainerToList(foundObjectsInTrigger[i].gameObject);
            }

            if (foundObjectsInTrigger[i].gameObject.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.DroppedItem)
            {
                collisionBoxReference.AddItemToPhysicalList(foundObjectsInTrigger[i].gameObject);
            }
        }
    }

    public void OnLoad()
    {
        ItemsToUse.Clear();

        collisionBoxReference.itemsWithinBox.Clear();

        collisionBoxReference.containersNearby.Clear();

        foundObjectsInTrigger = Physics.OverlapBox(triggerZone.bounds.center, triggerZone.bounds.extents, triggerZone.transform.rotation, itemDetectionMask);

        collisionBoxReference.gameObjectsWithinBox.RemoveAll(GameObject => GameObject == null);

        collisionBoxReference.containersNearby.RemoveAll(Container => Container == null);


        foundObjectsInTrigger = new Collider[0];

        for (int i = 0; i < foundObjectsInTrigger.Length; i++)
        {
            if (foundObjectsInTrigger[i].gameObject.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.Container)
            {
                collisionBoxReference.AddContainerToList(foundObjectsInTrigger[i].gameObject);
            }

            if (foundObjectsInTrigger[i].gameObject.GetComponent<SaveableItem>().thisSaveType == SaveableItemType.DroppedItem)
            {
                collisionBoxReference.AddItemToPhysicalList(foundObjectsInTrigger[i].gameObject);
            }
        }
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (playerNearby)
            {
                MakeItemFromRecipe();
            }
        }
    }

    public void MakeItemFromRecipe()
    {
        ghostDestroyList.Clear();

        output = Inventory._InvRef.activeRecipe;

        collisionBoxReference.gameObjectsWithinBox.RemoveAll(GameObject => GameObject == null);

        collisionBoxReference.containersNearby.RemoveAll(Container => Container == null);

        ItemsToUse.Clear();

        for (int i = 0; i < collisionBoxReference.itemsWithinBox.Count; i++)
        {
            ItemsToUse.Add(collisionBoxReference.itemsWithinBox[i]);
        }

        for (int i = 0; i < collisionBoxReference.containersNearby.Count; i++)
        {
            for (int x = 0; x < collisionBoxReference.containersNearby[i].containerContents.Length; x++)
            {
                if (collisionBoxReference.containersNearby[i].containerContents[x].ID != -1 && !ItemsToUse.Contains(collisionBoxReference.containersNearby[i].containerContents[x]))
                {
                    ItemsToUse.Add(collisionBoxReference.containersNearby[i].containerContents[x]);
                }
            }
        }

        if (output != null)
        {
            //this snipet of code removes any null gameobjects from the list, that were removed somehow without also removing themselve from the list

            if (hasRequiredItems())
            {

                GameObject clone;
                Item result;
                result = ItemDatabase.FetchByID(output.resultID);
                clone = Instantiate(Resources.Load("DropModels/" + result.DropModel, typeof(GameObject))) as GameObject;

                clone.transform.position = workbenchOutputLocation.position;

                clone.transform.rotation = workbenchOutputLocation.rotation;

                // clone.GetComponent<AddOnClick>().DroppedItem = true;
                clone.GetComponent<AddOnClick>().thisItem = result;

                Inventory._InvRef.containerMangerRef.RefreshContainer();
            }
        }
    }

    //ID Based Syste,
    //public bool hasRequiredItems()
    //{
    //    //a temporary list of items that we did use for crafting, that will be deleted when we are done
    //    List<ItemAndAmmountReference> ghostDestroyList = new List<ItemAndAmmountReference>();

    //    //the list of items we have inside the box/near the workbench to work with/cycle through to see if we have the right shit.
    //    List<Item> referenceList = collisionBoxReference.itemsWithinBox;

    //    //for the number of components we need to get, run this code that many times, so pretty much a cycle through all the components we need.

    //    //lets say that the first componet we need is a rock.
    //    for (int x = 0; x < output.components.Length; x++)
    //    {

    //        //The items we have available to craft with
    //        //as well as an empty number to count how many total we have of something
    //        int totalAmmountCounted = 0;
    //        //cycle through our item we know we have

    //        //if we dont even have the id of the component we are cycling on, the return false
    //        if (!hasID(output.components[x].ID))
    //        {
    //            return false;
    //        }

    //        //cycle through every item that we have within that box/range or the workbench
    //        for (int i = 0; i < referenceList.Count; i++)
    //        {
    //            //if the id of the item we have is equal to the first component of the recipe
    //            if (referenceList[i].ID == output.components[x].ID)
    //            {
    //                //if we have enough of the item
    //                //we have 20 coins placed, we need 5, there should be 15 left over
    //                if (referenceList[i].StackSize >= output.components[x].Amount)
    //                {
    //                    //                                                                  20-5 = 15(we want it to be 5)
    //                    ghostDestroyList.Add(new ItemAndAmmountReference(referenceList[i], output.components[x].Amount, collisionBoxReference.gameObjectsWithinBox[i]));
    //                    //we know we have enough, or even more, so we set the total ammount we have to the max required.
    //                    totalAmmountCounted = output.components[x].Amount;
    //                    break;
    //                }
    //                //if we didnt have enough of the item from just the first item we found
    //                else
    //                {
    //                    //add the item we are cycling through to a total number stack size
    //                    ghostDestroyList.Add(new ItemAndAmmountReference(referenceList[i], referenceList[i].StackSize, collisionBoxReference.gameObjectsWithinBox[i]));
    //                    totalAmmountCounted += referenceList[i].StackSize;

    //                    if (totalAmmountCounted >= output.components[x].Amount)
    //                    {
    //                        break;
    //                    }
    //                }
    //            }
    //        }

    //        //if when we continued to look for the required items, we ended up finding them all
    //        if (totalAmmountCounted >= output.components[x].Amount)
    //        {
    //            //continue on.
    //            continue;
    //        }

    //        //otherwise we don't have what we need, therofore we can't create the item.
    //        else
    //        {
    //            return false;
    //        }
    //    }

    //    for (int i = 0; i < ghostDestroyList.Count; i++)
    //    {

    //        //stack size equals however much the crafting needed from that specific item
    //        ghostDestroyList[i].objToDestroy.GetComponent<AddOnClick>().Amnt -= ghostDestroyList[i].amountToRemember;

    //        //if the ammount is 0
    //        if (ghostDestroyList[i].objToDestroy.GetComponent<AddOnClick>().Amnt <= 0)
    //        {
    //            //then we actually destroy the item.
    //            collisionBoxReference.itemsWithinBox.Remove(ghostDestroyList[i].itemToRemember);
    //            collisionBoxReference.gameObjectsWithinBox.Remove(ghostDestroyList[i].objToDestroy);
    //            Destroy(ghostDestroyList[i].objToDestroy);
    //        }
    //    }

    //    return true;
    //}

        //ComponentBasedSystem
    public bool hasRequiredItems()
    {
        //for the number of components we need to get, run this code that many times, so pretty much a cycle through all the components we need.
        for (int x = 0; x < output.components.Length; x++)
        {
            //we start off with 0, we haven't found anything yet.
            int totalAmmountCounted = 0;

            //if we dont even have the first component of the recipe we are checking, then return false, we can't make it at all
            if (!hasComponent(output.components[x].componentType))
            {
                return false;
            }

            //cycle through every item that we have within that box/range or the workbench
            for (int i = 0; i < ItemsToUse.Count; i++)
            {

                for (int z = 0; z < ItemsToUse[i].componentsHave.Length; z++)
                {
                    //if the component of the item we are looking at is what we want
                    if (ItemsToUse[i].componentsHave[z].thisComponent == output.components[x].componentType)
                    {
                        //if we have more than enough of the componenet
                        if (ItemsToUse[i].componentsHave[z].amountOfThisComponent >= output.components[x].Amount)
                        {
                            //if i is greater than how many physical items we have in the box, then we should be looking at container items then
                            if (i < collisionBoxReference.itemsWithinBox.Count)
                            {

                                ghostDestroyList.Add(new ItemAndAmmountReference(ItemsToUse[i], output.components[x].Amount, collisionBoxReference.gameObjectsWithinBox[i]));
                                //we know we have enough, or even more, so we set the total amount we have to the max required.
                                totalAmmountCounted = output.components[x].Amount;
                                break;
                            }

                            else
                            {
                                ghostDestroyList.Add(new ItemAndAmmountReference(ItemsToUse[i], output.components[x].Amount));
                                //we know we have enough, or even more, so we set the total amount we have to the max required.
                                totalAmmountCounted = output.components[x].Amount;
                                break;
                            }
                        }
                    }
                }

                //we come back to this each time the for loop goes
                if (totalAmmountCounted >= output.components[x].Amount)
                {
                    //continue on.
                    break;
                }
            }

            //if when we continued to look for the required items, we ended up finding them all
            if (totalAmmountCounted >= output.components[x].Amount)
            {
                //continue on.
                continue;
            }

            //otherwise we don't have what we need, therofore we can't create the item.
            else
            {
                return false;
            }
        }

        for (int i = 0; i < ghostDestroyList.Count; i++)
        {

            if (ghostDestroyList[i].objToDestroy != null)
            {
                //stack size equals however much the crafting needed from that specific item
                ghostDestroyList[i].objToDestroy.GetComponent<AddOnClick>().Amnt -= ghostDestroyList[i].amountToRemember;
                ghostDestroyList[i].objToDestroy.GetComponent<AddOnClick>().thisItem.StackSize -= ghostDestroyList[i].amountToRemember;

                //if the ammount is 0
                if (ghostDestroyList[i].objToDestroy.GetComponent<AddOnClick>().Amnt <= 0)
                {
                    //then we actually destroy the item.
                    collisionBoxReference.itemsWithinBox.Remove(ghostDestroyList[i].itemToRemember);
                    collisionBoxReference.gameObjectsWithinBox.Remove(ghostDestroyList[i].objToDestroy);
                    Destroy(ghostDestroyList[i].objToDestroy);
                }
            }

            else
            {
                ghostDestroyList[i].itemToRemember.StackSize -= ghostDestroyList[i].amountToRemember;

                if (ghostDestroyList[i].itemToRemember.StackSize <= 0)
                {
                    ghostDestroyList[i].itemToRemember.ID = -1;
                }
            }
        }

        return true;
    }

    public class ItemAndAmmountReference
    {
        public GameObject objToDestroy;
        public Item itemToRemember;
        public int amountToRemember;

        public ItemAndAmmountReference(Item _ItemToRemember, int _ammountToRemember, GameObject _objToDestroy)
        {
            itemToRemember = _ItemToRemember;
            amountToRemember = _ammountToRemember;
            objToDestroy = _objToDestroy;
        }

        public ItemAndAmmountReference(Item _ItemToRemember, int _ammountToRemember)
        {
            itemToRemember = _ItemToRemember;
            amountToRemember = _ammountToRemember;
        }
    }

    public bool hasComponent(ComponentType componentToLookFor)
    {
        for (int i = 0; i < ItemsToUse.Count; i++)
        {
            for (int x = 0; x < ItemsToUse[i].componentsHave.Length; x++)
            {
                if (ItemsToUse[i].componentsHave[x].thisComponent == componentToLookFor)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool hasID(int id)
    {
        List<Item> referenceList = collisionBoxReference.itemsWithinBox;
        for (int i = 0; i < referenceList.Count; i++)
        {
            if (referenceList[i].ID == id)
            {
                return true;
            }
        }
        return false;
    }
}
