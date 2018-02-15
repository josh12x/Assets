using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerManager : MonoBehaviour
{
    [Header("References")]

    public Inventory InvRef;

    [Header("ContainerStuff")]
    public Transform containerSlotParent;
    public Text ContainerName;
    public Slot[] containerSlots;

    public Item[] openedContainerContent;

    public Item[] displayedContainerContent;
    public int containerItemsPerPage;
    public int currentNumberOfContainerPages;
    public int currentContainerPage;
    public Transform openContainerPos;
    public Container currentlyOpenedContainer;
    public bool isContainerOpen;

    public void Start()
    {
        InvRef = GetComponent<Inventory>();
        InitializeContainer();
    }

    public void Update()
    {
        //just a quick little way to constantly check to see if the player is within a certain distance of the container, if they aren't, then automagicaly close it.
        if (isContainerOpen)
        {
            CloseContainerDistanceUpdate();
        }
    }

    public void InitializeContainer()
    {
        containerItemsPerPage = containerSlotParent.childCount;

        containerSlots = new Slot[containerItemsPerPage];

        for (int i = 0; i < containerItemsPerPage; i++)
        {
            containerSlots[i] = containerSlotParent.GetChild(i).GetComponent<Slot>();
            containerSlots[i].index = i;
        }

        displayedContainerContent = new Item[20];

    }

    public void OpenContainerInteraction()
    {
        OpenContainer();
    }

    public void RefreshContainer()
    {
        currentNumberOfContainerPages = Mathf.CeilToInt((float)openedContainerContent.Length / (float)containerItemsPerPage) - 1;

        if (openedContainerContent.Length - (currentContainerPage * containerItemsPerPage) >= containerItemsPerPage)
        {
            for (int i = 0; i < containerItemsPerPage; i++)
            {
                displayedContainerContent[i] = openedContainerContent[i + (currentContainerPage * containerItemsPerPage)];
            }

            for (int i = 0; i < containerItemsPerPage; i++)
            {
                InvRef.UpdateAllSlotsFromArray(containerSlots, displayedContainerContent);
                containerSlots[i].gameObject.SetActive(true);
            }

        }

        else
        {
            for (int i = 0; i < openedContainerContent.Length - (currentContainerPage * containerItemsPerPage); i++)
            {
                displayedContainerContent[i] = openedContainerContent[i + (currentContainerPage * containerItemsPerPage)];
            }

            for (int i = 0; i < openedContainerContent.Length - (currentContainerPage * containerItemsPerPage); i++)
            {
                InvRef.UpdateAllSlotsFromArray(containerSlots, displayedContainerContent);
            }

            for (int i = openedContainerContent.Length - (currentContainerPage * containerItemsPerPage); i < containerItemsPerPage; i++)
            {
                containerSlots[i].gameObject.SetActive(false);
            }
        }
    }

    public void CloseContainerDistanceUpdate()
    {
        if (isContainerOpen && openContainerPos != null)
        {
            if (Vector3.Distance(InvRef.playerTransform.position, openContainerPos.position) > InvRef.objectInteractionRange)
            {
                CloseContainer();
            }
        }
    }

    public void CloseContainer()
    {
        if (currentlyOpenedContainer != null)
        {
            currentlyOpenedContainer.Interact(false);
        }

        isContainerOpen = false;
        openContainerPos = null;
        InvRef.ToggleContainerMenu(false);
        currentlyOpenedContainer = null;
    }

    public void OpenContainer()
    {
        currentlyOpenedContainer = InvRef.worldInteractionHit.transform.GetComponent<Container>();

        openedContainerContent = currentlyOpenedContainer.containerContents;

        openContainerPos = currentlyOpenedContainer.transform;

        currentlyOpenedContainer.Interact(true);

        isContainerOpen = true;

        ContainerName.text = currentlyOpenedContainer.containerName;

        InvRef.ToggleContainerMenu(true);

        RefreshContainer();

        InvRef.clickTimer = 0;
    }

    public void RenameContainer(string newName)
    {
        currentlyOpenedContainer.containerName = newName;
    }

    public void NextPage()
    {
        NextPageMaster(ref containerItemsPerPage, ref currentNumberOfContainerPages, ref currentContainerPage, ref containerSlots, ref displayedContainerContent, ref openedContainerContent);
    }

    public void NextPageMaster(ref int itemsPerPage, ref int currentNumberOfPages, ref int currentPage, ref Slot[] slotsToUpdate, ref Item[] itemsToShowOnPage, ref Item[] MasterItems)
    {
        if (currentPage <= currentNumberOfPages)
        {
            currentPage += 1;
        }

        if (currentPage > currentNumberOfPages)
        {
            currentPage = 0;
        }

        RefreshContainer();
    }
}
