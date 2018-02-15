using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorPickerWindow : MonoBehaviour
{
    public Slider red;
    public Slider green;
    public Slider blue;
    public Slider alpha;

    public UIWindow activeWindow;

    public Color currentMadeColor;

    public Color displayColor;

    public Image displayImage;

    public Text currentChangingElementName;

    //0 is changing color of lighter elements,
    //1 is darker,
    //2 is for text
    //3 is for slots
    public int state;


    public void ChangeColor()
    {
        currentMadeColor.r = red.value;
        currentMadeColor.b = blue.value;
        currentMadeColor.g = green.value;
        currentMadeColor.a = alpha.value;

        if (gameObject.activeInHierarchy)
        {

            if (state == 0)
            {
                activeWindow.darkColor = currentMadeColor;

            }


            if (state == 1)
            {
                activeWindow.dragBarColor = currentMadeColor;
            }


            if (state == 2)
            {
                activeWindow.ChestColor = currentMadeColor;
            }


            if (state == 3)
            {
                activeWindow.lightColor = currentMadeColor;
            }

            if (state == 4)
            {
                activeWindow.slotColor = currentMadeColor;
            }

            if (state == 5)
            {
                activeWindow.textColor = currentMadeColor;
            }


            activeWindow.UpdateAllColor();

            if (Inventory._InvRef.containerMangerRef.isContainerOpen)
            {
                Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[0] = activeWindow.darkColor;

                Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[1] = activeWindow.dragBarColor;

                Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[2] = activeWindow.ChestColor;

                Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[3] = activeWindow.lightColor;

                Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[4] = activeWindow.slotColor;

                Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[5] = activeWindow.textColor;
            }
        }
    }

    public void SliderChange()
    {
        displayColor.r = red.value;
        displayColor.b = blue.value;
        displayColor.g = green.value;
        displayColor.a = alpha.value;

        displayImage.color = displayColor;
    }


    public void GetCurrentValues()
    {
        activeWindow = Inventory._InvRef.hoveringWindow;
        if (gameObject.activeInHierarchy)
        {
            if (state == 0)
            {
                currentChangingElementName.text = "Color 1";
                red.value = activeWindow.darkColor.r;
                blue.value = activeWindow.darkColor.b;
                green.value = activeWindow.darkColor.g;
                alpha.value = activeWindow.darkColor.a;
            }

            if (state == 1)
            {
                currentChangingElementName.text = "TitleBar Color";
                red.value = activeWindow.dragBarColor.r;
                blue.value = activeWindow.dragBarColor.b;
                green.value = activeWindow.dragBarColor.g;
                alpha.value = activeWindow.dragBarColor.a;
            }

            if (state == 2)
            {
                currentChangingElementName.text = "Chest Color";
                if (Inventory._InvRef.containerMangerRef.isContainerOpen)
                {
                    red.value = Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[2].r;
                    blue.value = Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[2].b;
                    green.value = Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[2].g;
                    alpha.value = Inventory._InvRef.containerMangerRef.currentlyOpenedContainer.containerColors[2].a;
                }
            }


            if (state == 3)
            {
                currentChangingElementName.text = "Color 2";
                red.value = activeWindow.lightColor.r;
                blue.value = activeWindow.lightColor.b;
                green.value = activeWindow.lightColor.g;
                alpha.value = activeWindow.lightColor.a;
            }

            if (state == 4)
            {
                currentChangingElementName.text = "Slot Color";
                red.value = activeWindow.slotColor.r;
                blue.value = activeWindow.slotColor.b;
                green.value = activeWindow.slotColor.g;
                alpha.value = activeWindow.slotColor.a;
            }


            if (state == 5)
            {
                currentChangingElementName.text = "Text Color";
                red.value = activeWindow.textColor.r;
                blue.value = activeWindow.textColor.b;
                green.value = activeWindow.textColor.g;
                alpha.value = activeWindow.textColor.a;
            }
        }
    }

    public void ChangeState()
    {
        state += 1;


        if (state == 2 && Inventory._InvRef.containerMangerRef.isContainerOpen == false)
        {
            state = 3;
        }

        if (state > 5)
        {
          state = 0;
        }

        GetCurrentValues();
    }
}
