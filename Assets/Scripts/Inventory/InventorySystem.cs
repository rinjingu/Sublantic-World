using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// The InventorySystem class manages the player's inventory.
/// </summary>
public class InventorySystem : MonoBehaviour
{

    public List<ItemInstance> inventory = new List<ItemInstance>();

    private void Awake(){}

    /// <summary>
    /// Adds an item to the inventory.
    /// </summary>
    /// <param name="item">The item to add.</param>
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

    /// <summary>
    /// Removes an item from the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
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

    /// <summary>
    /// Removes all instances of an item from the inventory.
    /// </summary>
    /// <param name="item">The item to remove.</param>
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

    /// <summary>
    /// Checks if the inventory contains an item.
    /// </summary>
    /// <param name="item">The item to check for.</param>
    /// <returns>True if the inventory contains the item, false otherwise.</returns>
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

    /// <summary>
    /// Removes an item instance from the inventory.
    /// </summary>
    /// <param name="item">The item instance to remove.</param>
    public void RemoveInstance(ItemInstance item)
    {
        inventory.Remove(item);
    }
}
