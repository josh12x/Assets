using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public HoverType thisHoverType;
    public UIElementType thisElementType;
    public UIWindow AttachedWindow;


    public virtual void OnPointerEnter(PointerEventData data)
    {
        if (thisElementType == UIElementType.Default)
        {
            Inventory.ToggleHover(thisHoverType, true);
        }

        if (thisElementType == UIElementType.GivePosition)
        {
            Inventory.ToggleHoverWithPosition(thisHoverType, transform.position, true);
        }


        if (thisElementType == UIElementType.GiveWindow)
        {
            Inventory.ToggleHoverWithPositionAndWindow(thisHoverType, transform.position, true, AttachedWindow);
        }

        if (thisElementType == UIElementType.ConstructTooltip)
        {
            Inventory.ToggleHover(thisHoverType, true);
        }
    }

    public virtual void OnPointerExit(PointerEventData data)
    {
        Inventory.ToggleHover(HoverType.Nothing, false);
    }

}

public enum UIElementType
{
    Default,
    GivePosition,
    GiveWindow,
    ConstructTooltip
}
