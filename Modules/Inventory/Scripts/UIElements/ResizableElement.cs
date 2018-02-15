using UnityEngine;

[System.Serializable]
public class ResizeableElement
{
    [SerializeField]
    public Vector2 sizePadding;

    [SerializeField]
    public RectTransform windowToResize;

    public ResizeableElement(Vector2 size, RectTransform window)
    {
        sizePadding = size;
        windowToResize = window;
    }
}
