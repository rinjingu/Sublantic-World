using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, InputSystem.IPlayerControlActions
{
    private InventorySystem inventory;

    private InputSystem inputSystem;
    private MovementController movementController;
    private GameObject InventoryUI;



    private void OnEnable()
    {
        Debug.Log("Start loading Player System");

        inventory = GetComponent<InventorySystem>();
        InventoryUI = this.transform.Find("UICanvas/InventoryUI").gameObject;
        //disable the inventory ui on start
        InventoryUI.SetActive(false);

        // initialize the player input system
        if (inputSystem == null)
        {
            inputSystem = new InputSystem();
            inputSystem.PlayerControl.SetCallbacks(this);
            inputSystem.Enable();
        }

        movementController = GetComponent<MovementController>();

        Debug.Log("Finished loading Player System");
        
        SetCursorLock(true);
    }

    private void Update()
    {
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        movementController.moveVector = context.ReadValue<Vector3>();
    }

    public void OnRotate(InputAction.CallbackContext context)
    {
        Debug.Log("OnRotate");
        movementController.Rotate(context.ReadValue<Vector2>());

    }

    public void OnAdjustSpeed(InputAction.CallbackContext context)
    {
        Debug.Log("OnAdjustSpeed");
    }

    public void OnOpenCloseInventory(InputAction.CallbackContext context)
    {
        InventoryUI.SetActive(!InventoryUI.activeSelf);
        SetCursorLock(!InventoryUI.activeSelf);
    }

    private void SetCursorLock(bool locked)
    {
        if (locked)
        {
            UnityEngine.Cursor.lockState = CursorLockMode.Locked;
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
        else
        {
            UnityEngine.Cursor.lockState = CursorLockMode.None;
            UnityEngine.Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        }
    }
}
