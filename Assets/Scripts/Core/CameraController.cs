using UnityEngine;


public class CameraController : MonoBehaviour {
    private GameObject MainCamera;
    private GameObject PlayerObject;
    private void OnEnable() {
        MainCamera = this.transform.Find("Main Camera").gameObject;
        if (MainCamera == null) {
            Debug.LogError("Main Camera not found");
        }
        PlayerObject = this.transform.Find("PlayerObject").gameObject;
        if (PlayerObject == null) {
            Debug.LogError("PlayerObject not found");
        }
    }
    private void Update() {
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