using UnityEngine;
using UnityEngine.InputSystem;

public class MovementController : MonoBehaviour
{
    public Vector3 moveVector;
    public Vector2 rotateVector;
    public float power;
    private GameObject playerSystem;
    private Rigidbody playerRigidBody;

    private void OnEnable()
    {
        Debug.Log("MovementController enabled");
        moveVector = Vector3.zero;
        rotateVector = Vector2.zero;
        playerSystem = this.transform.Find("PlayerSystem").gameObject;
        if(playerSystem == null)
        {
            Debug.LogError("PlayerSystem not found");
        }else{
            playerRigidBody = playerSystem.transform.Find("PlayerObject").gameObject.GetComponent<Rigidbody>();
        }
        
    }

    private void Update()
    {
        Move(moveVector);
    }

    private void Move(Vector3 vector)
    {
        var currentPosition = playerRigidBody.position;
        var currentRotation = playerRigidBody.rotation;
        // map local axis to world axis
        var vehicaleForce = playerSystem.GetComponent<UVehicleController>().forceComposite;
        var worldAxis = currentRotation * vector;
        playerRigidBody.MovePosition(currentPosition + worldAxis * Time.deltaTime);
    }

    public void Rotate(Vector2 vector)
    {
        var vector3 = new Vector3(vector.y, vector.x, 0);
        // convert the current rotation quaternion to euler angles
        var currentRotation = playerRigidBody.rotation;
        // find the worldaxis transformed from the input vector
        var worldAxis = currentRotation * vector3;
        var newRotation = Quaternion.Euler(worldAxis * 0.1f);
        // convert the new rotation to a quaternion
        var newQuaternion = newRotation * currentRotation;
        // set the new rotation
        playerRigidBody.MoveRotation(newQuaternion);
    }
}
