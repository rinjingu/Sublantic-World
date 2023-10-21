using UnityEngine;
using UnityEngine.UIElements;

public class ItemVisual : VisualElement
{
    private readonly ItemData itemData;

    public ItemVisual(ItemData itemData)
    {
        this.itemData = itemData;
        name = $"itemData.name";
        style.visibility = Visibility.Hidden;

        VisualElement icon = new VisualElement
        {
            style = { backgroundImage = itemData.icon.texture }
        };
        Add(icon);
        icon.AddToClassList("item-icon");
        AddToClassList("visual-item-container");
    }

}
