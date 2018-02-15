using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomizeBasicItem : RandomizeItemStats
{

    public int minValue;
    public int maxValue;

    public int minStackSize;
    public int maxStackSize;

	void Start ()
    {
        itemDroppedAttachedTo = GetComponent<AddOnClick>();

        if (itemDroppedAttachedTo.DroppedItem == false)
        {
            itemDroppedAttachedTo.Amnt = Random.Range(minStackSize, maxStackSize);

            itemDroppedAttachedTo.thisItem.StackSize = itemDroppedAttachedTo.Amnt;

            itemDroppedAttachedTo.thisItem.Value = Random.Range(minValue, maxValue);
        }
	}
	
}
