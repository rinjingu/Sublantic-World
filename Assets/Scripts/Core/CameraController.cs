using Unity.VisualScripting;
using UnityEngine;


public class CameraController : MonoBehaviour {
    private GameObject MainCamera;
    private GameObject PlayerObject;
    private void OnEnable() {
        MainCamera = this.transform.Find("Main Camera").gameObject;
        if (MainCamera == null) {
            Debug.LogError("Main Camera not found");
        }
        var playerTransform = transform.Find("PlayerObject");
        if (playerTransform == null) {
            Debug.LogError("PlayerObject not found");
            PlayerObject = null;
        }else{
            PlayerObject = playerTransform.gameObject;
        }
    }
    private void Update() {
        // update the PlayerObject
        var playerTransform = transform.Find("PlayerObject");
        if (playerTransform == null) {
            Debug.LogError("PlayerObject not found");
            PlayerObject = null;
        }else{
            PlayerObject = playerTransform.gameObject;
        }
        if (MainCamera == null || PlayerObject == null) {
            return;
        }
        var playerPosition = PlayerObject.transform.position;
        var cameraDistance = 5f;
        var cameraHeight = 1f;
        var cameraOffset = new Vector3(0, cameraHeight, -cameraDistance);
        cameraOffset = PlayerObject.transform.rotation * cameraOffset;
        var newCameraPosition = playerPosition + cameraOffset;
        MainCamera.transform.position = newCameraPosition;
        MainCamera.transform.rotation = PlayerObject.transform.rotation;
    }
}