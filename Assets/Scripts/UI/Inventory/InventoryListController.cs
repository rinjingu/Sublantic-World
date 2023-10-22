using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class InventoryListController
{
    private InventorySystem inventorySystem;
    private VisualTreeAsset listElementTemplate;

    VisualElement InventoryContainer;
    ListView itemListView;
    Label itemNameLabel;
    Label itemCountLabel;
    VisualElement itemVisualContainer;
    Button removeButton;

    public InventoryListController(
        VisualElement root,
        VisualTreeAsset listElementTemplate,
        InventorySystem inventorySystem
    )
    {
        this.inventorySystem = inventorySystem;
        this.listElementTemplate = listElementTemplate;

        InventoryContainer = root.Q<VisualElement>("Inventory");

        itemListView = root.Q<ListView>("InventoryList");

        itemNameLabel = root.Q<Label>("InfoName");
        itemCountLabel = root.Q<Label>("InfoCount");
        itemVisualContainer = root.Q<VisualElement>("InfoIcon");

        ClearInfoPanel();

        FillItemListView();

        removeButton = root.Q<Button>("RemoveButton");

        removeButton.clicked += OnRemoveButtonClicked;

        itemListView.onSelectionChange += OnEntrySelected;
    }

    private void FillItemListView()
    {
        itemListView.makeItem = () =>
        {
            var newListEntry = listElementTemplate.Instantiate();
            var newListEntryController = new InventoryListEntryController();

            newListEntry.userData = newListEntryController;
            newListEntryController.SetVisualElement(newListEntry, InventoryContainer);

            return newListEntry;
        };

        itemListView.bindItem = (e, i) =>
        {
            (e.userData as InventoryListEntryController).SetItemInstance(
                inventorySystem.inventory[i]
            );
        };

        itemListView.fixedItemHeight = 45;

        itemListView.itemsSource = inventorySystem.inventory;
    }

    private void OnEntrySelected(IEnumerable<object> selectedItems)
    {
        RefreshInfoPanel();
    }

    private void ClearInfoPanel()
    {
        itemNameLabel.text = "";
        itemCountLabel.text = "";
        itemVisualContainer.style.backgroundImage = null;
    }

    private void OnRemoveButtonClicked()
    {
        var selectedItem = itemListView.selectedItem as ItemInstance;

        if (selectedItem == null)
        {
            Debug.LogWarning("No item selected");
            return;
        }

        inventorySystem.RemoveItem(selectedItem);
        itemListView.Rebuild();
        RefreshInfoPanel();
    }

    private void RefreshInfoPanel(){
        var selectedItem = itemListView.selectedItem as ItemInstance;
        if (selectedItem == null)
        {
            //clear info panel
            ClearInfoPanel();

            return;
        }

        //Fill info panel
        itemNameLabel.text = selectedItem.itemType.name;
        itemCountLabel.text = selectedItem.count.ToString();
        itemVisualContainer.style.backgroundImage = selectedItem.itemType.icon.texture;
    }
}
