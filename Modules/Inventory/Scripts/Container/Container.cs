using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LitJson;
using System.IO;
using UnityEngine.UI;
using System.Linq;
using System.Text;
using System;


public class Container : AddOnClick
{
    [Tooltip("How many items can this container hold?")]
    public int containerSpace;

    public Item[] containerContents;

    public Animation containerAnimations;

    public Workbench activeBench;

    bool Open;

    public string containerName;

    public Vector3 containerUIposition;

    public Color[] containerColors;


    void Awake ()
    {
        containerContents = new Item[containerSpace];

        for (int i = 0; i < containerSpace; i++)
        {
            containerContents[i] = new Item();
        }

        //containerContents[0] = ItemDatabase.FetchByID(5);
	}

    public void Start()
    {

        if (DroppedItem == false)
        {
            if (createItemAtStartFromDatabase)
            {
                thisItem = Inventory._InvRef.TypeChecker(ItemDatabase.FetchByID(id));
                thisItem.StackSize = Amnt;
            }
        }
        Debug.Log(thisItem.DropModel);

        //activeWindow.ChestColor = currentMadeColor;
        if (GetComponentInChildren<SkinnedMeshRenderer>() != null)
        {
            Mesh mesh = Instantiate(GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh);
            Color[] colors = mesh.colors;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = containerColors[2];
            }

            mesh.colors = colors;
            GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = mesh;
        }

        else if (GetComponentInChildren<MeshFilter>() != null)
        {
            Mesh mesh = Instantiate(GetComponentInChildren<MeshFilter>().sharedMesh);
            Color[] colors = mesh.colors;

            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = containerColors[2];
            }

            mesh.colors = colors;
            GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
        }
    }

    public void Interact(bool state)
    {
        Open = state;
        if (Open)
        {
            containerAnimations.Play("Open");
            Inventory._InvRef.UIWindowContainer.darkColor = containerColors[0];
            Inventory._InvRef.UIWindowContainer.dragBarColor = containerColors[1];
            Inventory._InvRef.UIWindowContainer.ChestColor = containerColors[2];
            Inventory._InvRef.UIWindowContainer.lightColor = containerColors[3];
            Inventory._InvRef.UIWindowContainer.slotColor = containerColors[4];
            Inventory._InvRef.UIWindowContainer.textColor = containerColors[5];

            Inventory._InvRef.UIWindowContainer.UpdateAllColor();
        }
        else
        {
            containerAnimations.Play("Close");
        }
    }

    public bool IsEmpty()
    {
        for (int i = 0; i < containerContents.Length; i++)
        {
            if (containerContents[i].ID != -1)
            {
                return false;
            }
        }

        return true;
    }
}
