using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private InventorySystem inventory;
    private GameObject InventoryUI;
    private List<string> pressedKeys = new List<string>();

    [ReadOnly]
    [SerializeField]
    private InputAction InventoryKey;

    [ReadOnly]
    [SerializeField]
    private InputActionMap MovementKeys;

    private void Start()
    {
        Debug.Log("Start loading Player System");

        inventory = GetComponent<InventorySystem>();
        InventoryUI = this.transform.Find("UICanvas/InventoryUI").gameObject;
        //disable the inventory ui on start
        InventoryUI.SetActive(false);

        //START BIND INPUTS
        //bind the inventory key to the inventory ui
        InventoryKey = new InputAction("InventoryKey", binding: "<Keyboard>/i");
        InventoryKey.performed += ctx => ToggleInventoryUI();
        InventoryKey.Enable();

        //bind movement keys
        MovementKeys = new InputActionMap("MovementKeys");
        MovementKeys.AddAction("MoveFront", binding: "<Keyboard>/w");
        MovementKeys.AddAction("MoveBack", binding: "<Keyboard>/s");
        MovementKeys.AddAction("MoveLeft", binding: "<Keyboard>/a");
        MovementKeys.AddAction("MoveRight", binding: "<Keyboard>/d");

        RegisterKeyPressToAction(MovementKeys["MoveFront"], "w");
        RegisterKeyPressToAction(MovementKeys["MoveBack"], "s");
        RegisterKeyPressToAction(MovementKeys["MoveLeft"], "a");
        RegisterKeyPressToAction(MovementKeys["MoveRight"], "d");

        MovementKeys.Enable();
        //END BIND INPUTS
        Debug.Log("Finished loading Player System");
    }
    
    private void Update() {
        if (pressedKeys.Count > 0)
        {
            Debug.Log("Pressed keys: " + string.Join(", ", pressedKeys));
        }
    }

    private void ToggleInventoryUI()
    {
        Debug.Log("ToggleInventoryUI");
        InventoryUI.SetActive(!InventoryUI.activeSelf);
    }

    private void RegisterKeyPressToAction(InputAction inputAction, string key)
    {
        inputAction.started += ctx => pressedKeys.Add(key);
        inputAction.canceled += ctx => pressedKeys.Remove(key);
    }


}
