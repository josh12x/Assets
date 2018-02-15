using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIWindow : MonoBehaviour
{
    public WindowType thisWindowType;

    public Color darkColor;
    public Color dragBarColor;
    public Color ChestColor;
    public Color lightColor;
    public Color slotColor;
    public Color textColor;

    public Image[] dragBar;

    public Image[] darkerImages;

    public Image[] lighterImages;

    public Text[] windowTexts;

    public Image[] slotImages;

    public Text windowName;

    public Text DescriptionName;

    public Text DesciptionBase;

    public RectTransform iconTransform;

    public RectTransform mainWindowRectTransform;

    public void UpdateColor(int state)
    {
        if (state == 0)
        {
            for (int i = 0; i < darkerImages.Length; i++)
            {
                darkerImages[i].color = darkColor;
            }
        }

        if (state == 1)
        {
            for (int i = 0; i < dragBar.Length; i++)
            {
                dragBar[i].color = dragBarColor;
            }
        }

        if (state == 2)
        {
            if (Inventory._InvRef.containerMangerRef.isContainerOpen)
            {
                //activeWindow.ChestColor = currentMadeColor;
                if (Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                {
                    Mesh mesh = Instantiate(Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh);
                    Color[] colors = mesh.colors;

                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = ChestColor;
                    }

                    mesh.colors = colors;
                    Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh = mesh;
                }

                else if (Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.GetComponentInChildren<MeshFilter>() != null)
                {
                    Mesh mesh = Instantiate(Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.GetComponentInChildren<MeshFilter>().sharedMesh);
                    Color[] colors = mesh.colors;

                    for (int i = 0; i < colors.Length; i++)
                    {
                        colors[i] = ChestColor;
                    }

                    mesh.colors = colors;
                    Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.GetComponentInChildren<MeshFilter>().sharedMesh = mesh;
                }
            }
        }

        if (state == 3)
        {
            for (int i = 0; i < lighterImages.Length; i++)
            {
                lighterImages[i].color = lightColor;
            }
        }

        if (state == 4)
        {
            for (int i = 0; i < slotImages.Length; i++)
            {
                slotImages[i].color = slotColor;
            }
        }

        if (state == 5)
        {
            for (int i = 0; i < windowTexts.Length; i++)
            {
                windowTexts[i].color = textColor;
            }
        }
    }

    public void UpdateAllColor()
    {
        for (int i = 0; i < 6; i++)
        {
            UpdateColor(i);
        }
    }
}

public enum WindowType
{
    Container,
    Inventory,
    SaveLoad,
    Description,
    Recipes
}
