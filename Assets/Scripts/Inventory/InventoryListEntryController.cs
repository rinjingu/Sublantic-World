using UnityEngine.UIElements;
public class InventoryListEntryController {
    Label NameLabel;
    Label CountLabel;

    public void SetVisualElement(VisualElement visualElement) {
        
        NameLabel = visualElement.Q<Label>("item-name-label");
        CountLabel = visualElement.Q<Label>("item-count-label");
    }

    public void SetItemInstance(ItemInstance itemInstance) {
        NameLabel.text = itemInstance.itemType.name;
        CountLabel.text = itemInstance.count.ToString();
    }
}