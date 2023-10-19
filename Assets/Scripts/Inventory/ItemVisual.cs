using UnityEngine;
using UnityEngine.UIElements;

public class ItemVisual : VisualElement
{
    private readonly ItemData itemData;

    public ItemVisual(ItemData itemData)
    {
        this.itemData = itemData;
        name = $"itemData.name";

        VisualElement icon = new VisualElement
        {
            style = { backgroundImage = itemData.icon.texture }
        };
        Add(icon);
        icon.AddToClassList("item-icon");
        AddToClassList("visual-item-container");
    }

    public void SetPosition(Vector2 position)
    {
        style.left = position.x;
        style.top = position.y;
    }
}
