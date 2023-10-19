using System;
using UnityEngine;
using UnityEngine.UIElements;

[Serializable]
public sealed class ItemInstance
{
    public ItemData itemType;
    public ItemVisual itemVisual;
    public int count;

    public ItemInstance(ItemData itemType, int count = 1)
    {
        this.itemType = itemType;
        this.count = count;
    }
}
