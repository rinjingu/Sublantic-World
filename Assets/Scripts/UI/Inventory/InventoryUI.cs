using UnityEngine;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour
{
    [SerializeField]
    VisualTreeAsset listEntryTemplate;

    InventorySystem inventorySystem;
    UIDocument inventoryUI;
    VisualElement root;
    InventoryListController inventoryListController;

    private void OnEnable()
    {
        inventorySystem = GameObject.Find("GameSystem").GetComponent<InventorySystem>();
        inventoryUI = GetComponent<UIDocument>();
        root = inventoryUI.rootVisualElement;
        inventoryListController = new InventoryListController(
            root,
            listEntryTemplate,
            inventorySystem
        );
    }

}
