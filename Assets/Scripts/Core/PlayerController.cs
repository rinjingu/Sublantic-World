using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private InventorySystem inventory;

    private void Start()
    {
        Debug.Log("Start loading Player System");
        inventory = GetComponent<InventorySystem>();
    }
}
