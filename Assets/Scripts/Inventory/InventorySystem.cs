using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UIElements;
using Cysharp.Threading.Tasks;
using System.Linq;
using System.Threading.Tasks;


public class InventorySystem : MonoBehaviour
{
    public static InventorySystem instance;
    private List<ItemInstance> inventory = new List<ItemInstance>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //Configure();
        }
        else if (instance != this)
        {
            Destroy(this);
        }
    }

    public void AddItem(ItemInstance item)
    {
        if (item.itemType.IsStackable())
        {
            while (item.count > 0)
            {
                foreach (ItemInstance i in inventory)
                {
                    if (i.itemType == item.itemType)
                    {
                        // add to existing stack without exceeding maxCount
                        if (i.count + item.count <= i.itemType.maxCount)
                        {
                            i.count += item.count;
                            return;
                        }
                        else
                        {
                            // add to existing stack
                            item.count -= i.itemType.maxCount - i.count;
                            i.count = i.itemType.maxCount;
                        }
                    }
                }

                // create new stack
                if (item.count > 0)
                {
                    ItemInstance newItem = new ItemInstance(item.itemType, item.itemType.maxCount);
                    item.count -= item.itemType.maxCount;
                    inventory.Add(newItem);
                }
            }

            return;
        }
        inventory.Add(item);
    }

    public void RemoveItem(ItemInstance item)
    {
        if (this.HasItem(item.itemType) == false)
        {
            return;
        }
        if (item.itemType.IsStackable())
        {
            foreach (ItemInstance i in inventory)
            {
                if (i.itemType == item.itemType)
                {
                    if (i.count > item.count)
                    {
                        i.count -= item.count;
                        return;
                    }
                    else
                    {
                        inventory.Remove(i);
                        item.count -= i.count;
                    }
                }
            }
        }
        else
        {
            inventory.Remove(item);
        }
    }

    public void RemoveAll(ItemData item)
    {
        foreach (ItemInstance i in inventory)
        {
            if (i.itemType == item)
            {
                inventory.Remove(i);
            }
        }
    }

    public bool HasItem(ItemData item)
    {
        foreach (ItemInstance i in inventory)
        {
            if (i.itemType == item)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveInstance(ItemInstance item)
    {
        inventory.Remove(item);
    }


}
