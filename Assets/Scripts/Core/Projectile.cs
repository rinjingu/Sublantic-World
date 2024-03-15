using UnityEngine;

public class Projectile : MonoBehaviour {
    public bool isDestroyable = false;
    public bool isMovable = false;
    [HideInInspector]
    public Weapon weapon;

    [SerializeField]
    private float m_ExistTime = 0f;
    private void OnCollisionEnter(Collision other) {
        weapon.OnHit(gameObject, other.gameObject);
    }

    private void Update() {
        m_ExistTime += Time.deltaTime;
        if (weapon.BulletType == ProjectileType.Missile){
            isMovable = m_ExistTime > weapon.MissileLaunchTime ? true : false;
            MissileTracking();
        }

        if (m_ExistTime > weapon.MaxLifeTime) {
            weapon.OnHit(gameObject, null);
        }
    }

    private void MissileTracking(){
        Rigidbody rb = GetComponent<Rigidbody>();
        Transform transform = GetComponent<Transform>();
        if (weapon.weaponController.Target == null || isMovable == false) {
            var direction = transform.forward;
            rb.velocity = direction.normalized * weapon.velocity;
            return;
        }


        // get the direction to the target
        var targetDirection = weapon.weaponController.Target.transform.position - transform.position;
        targetDirection.Normalize();

        // calculate the rotation to the target
        var rotation = Quaternion.LookRotation(targetDirection);

        // calculate the angle to the target
        var angle = Vector3.Angle(targetDirection, transform.forward);

        // calculate the time needed to turn to the target
        var time = angle / weapon.MissileTurnRate;
        var percentage = Time.deltaTime / time;
        // cal the rotation of the missile to the target
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, percentage);

        // get current velocity of the missile
        var speed = rb.velocity.magnitude;

        // calculate the velocity to the target
        var velocity = transform.forward * weapon.velocity;
        rb.velocity = velocity;
        
    }
}