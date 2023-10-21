using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class InventoryUI : MonoBehaviour {
    [SerializeField]
    VisualTreeAsset listEntryTemplate;
    private void OnEnable() {
        var inventoryUI = GetComponent<UIDocument>();
        var root = inventoryUI.rootVisualElement;
        var inventoryListController = new InventoryListController(root, listEntryTemplate, InventorySystem.instance);
    }
}