using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryListController {
    private InventorySystem inventorySystem;
    private VisualTreeAsset listElementTemplate;

    ListView itemListView;
    Label itemNameLabel;
    Label itemCountLabel;
    VisualElement itemVisualContainer;

    public InventoryListController(VisualElement root, VisualTreeAsset listElementTemplate, InventorySystem inventorySystem)
    {
        this.inventorySystem = inventorySystem;
        this.listElementTemplate = listElementTemplate;

        itemListView = root.Q<ListView>("InventoryList");

        itemNameLabel = root.Q<Label>("InfoName");
        itemCountLabel = root.Q<Label>("InfoCount");
        itemVisualContainer = root.Q<VisualElement>("InfoIcon");

        {foreach (var item in inventorySystem.inventory) {Debug.Log(item.itemType.name);}}
        FillItemListView();

        itemListView.onSelectionChange += OnEntrySelected;
    }
    
    private void FillItemListView()
    {
        itemListView.makeItem = () =>
        {
            var newListEntry = listElementTemplate.Instantiate();
            var newListEntryController = new InventoryListEntryController();

            newListEntry.userData = newListEntryController;
            newListEntryController.SetVisualElement(newListEntry);

            return newListEntry;
        };

        itemListView.bindItem = (e, i) =>
        {
            (e.userData as InventoryListEntryController).SetItemInstance(inventorySystem.inventory[i]);
        };

        itemListView.fixedItemHeight = 45;
        itemListView.itemsSource = inventorySystem.inventory;
    }
    
    private void OnEntrySelected(IEnumerable<object> selectedItems){
        var selectedItem = itemListView.selectedItem as ItemInstance;
        if (selectedItem == null){
            //clear info panel
            itemNameLabel.text = "";
            itemCountLabel.text = "";
            itemVisualContainer.style.backgroundImage = null;

            return;
        }

        //Fill info panel
        itemNameLabel.text = selectedItem.itemType.name;
        itemCountLabel.text = selectedItem.count.ToString();
        itemVisualContainer.style.backgroundImage = new StyleBackground(selectedItem.itemType.icon.texture);
    }
}