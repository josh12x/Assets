using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Slot : UIElement
{
    public int index;
    public Image thisSlotsItem;
    public TextMeshProUGUI stackText;
    public Image thisSlotBackround;
    public Sprite highlight;
    public Sprite normal;
    public bool empty = true;
    public bool haveThisRecipe;

    public override void OnPointerEnter(PointerEventData data)
    {
        Inventory.ToggleHover(thisHoverType, index, true);
        if (thisHoverType == HoverType.Inventory)
        {
            if (index > 9)
            {
                setHighlight();
            }
        }
    }

    public override void OnPointerExit(PointerEventData data)
    {
        Inventory.ToggleHover(thisHoverType, index, false);

        if (thisHoverType == HoverType.Inventory)
        {
            if (index > 9)
            {
                setNormal();
            }
        }
    }


    public void setHighlight()
    {
        if (highlight != null)
        {
            thisSlotBackround.sprite = highlight;
        }
    }

    public void setNormal()
    {
        if (normal != null)
        {
            thisSlotBackround.sprite = normal;
        }
    }
}
