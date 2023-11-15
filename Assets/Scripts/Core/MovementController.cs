using UnityEngine;
using UnityEngine.InputSystem;


public class MovementController : MonoBehaviour{
    public Vector3 moveVector;
    public Vector2 rotateVector;
    private GameObject playerObject;
    private Rigidbody rigidBody;
    private void OnEnable() {
        Debug.Log("MovementController enabled");
        moveVector = Vector3.zero;
        rotateVector = Vector2.zero;
        playerObject = this.transform.Find("Player").gameObject;
        rigidBody = playerObject.GetComponent<Rigidbody>();
    }
    

    private void Update() {
        Move(moveVector);
    }

    private void Move(Vector3 vector){
        rigidBody.MovePosition(playerObject.transform.position + vector * Time.deltaTime);
    }

    public void Rotate(Vector2 vector){
        var vector3 = new Vector3(vector.y, vector.x, 0);
        // convert the current rotation quaternion to euler angles
        var currentRotation = rigidBody.rotation;
        // add the new rotation to the current rotation
        var newRotation = Quaternion.Euler(vector3 * 0.1f);
        // convert the new rotation to a quaternion
        var newQuaternion = currentRotation * newRotation;
        // set the new rotation
        rigidBody.MoveRotation(newQuaternion);
    }
}