using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Resizer : MonoBehaviour,  IDragHandler
{
    public DragBar mainWindow;
    public bool vertical;

    public void OnDrag(PointerEventData data)
    {
    }
}