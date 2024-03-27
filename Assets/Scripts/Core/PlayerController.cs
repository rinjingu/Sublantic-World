using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.LowLevel;
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
        // get the playerSystem object in children
        var playerSystem = this.transform.Find("PlayerSystem").gameObject;
        if (playerSystem == null)
        {
            Debug.LogError("PlayerSystem not found");
            // instantiate a new playerSystem object from existing prefab
            playerSystem = Instantiate(Resources.Load<GameObject>("Prefabs/PlayerSystem"));
            playerSystem.transform.parent = this.transform;

        }
        var UVehicleController = playerSystem.GetComponent<UVehicleController>();
        if (UVehicleController == null)
        {
            Debug.LogError("UVehicleController not found");
            // instantiate a new UVehicleController object from existing prefab
            playerSystem.AddComponent<UVehicleController>();
            UVehicleController = playerSystem.GetComponent<UVehicleController>();
            // set the vehicleInstance from the existing prefab
            var temp = Resources.Load<GameObject>("Prefabs/PlayerSystem").GetComponent<UVehicleController>().vehicleInstance;
            UVehicleController.vehicleInstance = temp;
        }
        Debug.Log(UVehicleController.vehicleInstance.vehicleType);
        var vehiclePrefab = UVehicleController.vehicleInstance.vehicleType.vehiclePrefab;
        // check if the PlayerObject is not found
        var playerTransform = playerSystem.transform.Find("PlayerObject");
        if (playerTransform == null)
        {
            Debug.Log("PlayerObject not found");
            // instantiate a new playerObject from the vehiclePrefab
            playerTransform = Instantiate(vehiclePrefab).transform;
            playerTransform.parent = playerSystem.transform;
            playerTransform.name = "PlayerObject";
            // set the playerObject's position to the playerSystem's position
            playerTransform.position = playerSystem.transform.position;
        }
        var playerObject = playerTransform.gameObject;
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
        movementController.Rotate(context.ReadValue<Vector2>());
    }

    public void OnAdjustSpeed(InputAction.CallbackContext context)
    {
        Debug.Log("OnAdjustSpeed");
        context.ReadValue<float>();
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
