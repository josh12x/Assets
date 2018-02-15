using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DragBar : MonoBehaviour, IDragHandler, IPointerEnterHandler, IPointerExitHandler
{

    public bool ishoveringOver;

    public Transform thingToDrag;

    void Update()
    {
        if (ishoveringOver)
        {
            Vector3 newScale = thingToDrag.transform.localScale;
            newScale.x += Input.GetAxis("Mouse ScrollWheel") * .2f;
            newScale.y += Input.GetAxis("Mouse ScrollWheel") * .2f;
            thingToDrag.localScale = newScale;
            if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                thingToDrag.transform.position = Input.mousePosition;
                thingToDrag.SetAsLastSibling();
            }
        }
    }

    public void OnDrag(PointerEventData data)
    {
        //set the window we are draggin as the last sibling, so that i will appear in front of everything else.
        thingToDrag.SetAsLastSibling();

        int pushBack = 10;
        //if x is greater than our width, with a border of 10;
        if (thingToDrag.transform.position.x > Screen.width - 10)
        {
            thingToDrag.transform.position = new Vector3(thingToDrag.transform.position.x - pushBack, thingToDrag.transform.position.y, thingToDrag.transform.position.z);
        }

        if (thingToDrag.transform.position.x < 10)
        {
            thingToDrag.transform.position = new Vector3(thingToDrag.transform.position.x + pushBack, thingToDrag.transform.position.y, thingToDrag.transform.position.z);
        }

        if (thingToDrag.transform.position.y > Screen.height - 10)
        {
            thingToDrag.transform.position = new Vector3(thingToDrag.transform.position.x, thingToDrag.transform.position.y - pushBack, thingToDrag.transform.position.z);
        }

        if (thingToDrag.transform.position.y < 10)
        {
            thingToDrag.transform.position = new Vector3(thingToDrag.transform.position.x, thingToDrag.transform.position.y + pushBack, thingToDrag.transform.position.z);
        }
        
        //only move the window if our mouse is within the borders of the screen.
        if (Input.mousePosition.x < Screen.width - 10 && Input.mousePosition.x > 10 && Input.mousePosition.y < Screen.height - 10 && Input.mousePosition.y > 10)
        {
            thingToDrag.transform.position = Input.mousePosition;
        }

        //Used for switching the icon to the left or right side based on screen position.
        if (Input.mousePosition.x < Screen.width / 2)
        {
            Debug.Log("left side of screen");

            if(Inventory._InvRef)
            Inventory._InvRef.hoveringWindow.iconTransform.anchorMin = new Vector2(1, 0.5f);
            Inventory._InvRef.hoveringWindow.iconTransform.anchorMax = new Vector2(1, 0.5f);
            Inventory._InvRef.hoveringWindow.iconTransform.pivot = new Vector2(.5f, .5f);
        }

        if (Input.mousePosition.x > Screen.width / 2)
        {
            Debug.Log("Right side");
            Inventory._InvRef.hoveringWindow.iconTransform.anchorMin = new Vector2(0, 0.5f);
            Inventory._InvRef.hoveringWindow.iconTransform.anchorMax = new Vector2(0, 0.5f);
            Inventory._InvRef.hoveringWindow.iconTransform.pivot = new Vector2(.5f, .5f);
        }


    }

    public void OnPointerEnter(PointerEventData data)
    {
        ishoveringOver = true;
    }

    public void OnPointerExit(PointerEventData data)
    {
        ishoveringOver = false;
    }
}
    
