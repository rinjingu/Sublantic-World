using UnityEngine.UIElements;
public class InventoryListEntryController {

    VisualElement root;
    VisualElement ListEntryContainer;
    VisualElement IconContainer;
    Label NameLabel;
    Label CountLabel;

    public void SetVisualElement(VisualElement visualElement, VisualElement root) {
        this.root = root;
        ListEntryContainer = visualElement.Q<VisualElement>("list-entry-container");
        IconContainer = visualElement.Q<VisualElement>("item-icon-container");
        NameLabel = visualElement.Q<Label>("item-name-label");
        CountLabel = visualElement.Q<Label>("item-count-label");
    }

    public void SetItemInstance(ItemInstance itemInstance) {
        ListEntryContainer.style.width = root.style.width;
        IconContainer.style.backgroundImage = itemInstance.itemType.icon.texture;
        IconContainer.style.width = 32;
        IconContainer.style.height = 32;
        NameLabel.text = itemInstance.itemType.name;
        CountLabel.text = itemInstance.count.ToString();
    }
}