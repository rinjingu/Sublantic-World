using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "Sublantic World/Weapon", order = 0)]
public class Weapon : ScriptableObject {
    public string weaponName;
    public ProjectileType BulletType;
    public GameObject BulletPrefab;

    [Header("Speed Settings")]
    public float velocity = 20f;

    [Tooltip("The maximum range of the weapon")]
    public float MaxRange = 100f;

    [Header("Firerate Settings")]
    public float FireRate = 1f;
    public float FireCooldown = 0f;

    public float HeatRate = 10f;
    public float MaxHeat = 0f;

    

    [Header("Missile Settings")]
    //the follow properties are only aviailable wwhen the weapon is Missile
    [Tooltip("The turn rate of the missile in degrees per second")]
    public float MissileTurnRate = 10f; 
    [Tooltip("The initial launch time of the missile before it starts to track the target")]
    public float MissileLaunchTime = 1f;
    [Tooltip("The maximum life time of the missile additonally to the MaxRange / velocity")]
    public float MaxLifeTimeAddition = 10f;
    [HideInInspector]
    public float MaxLifeTime { get { return MaxRange / velocity + MaxLifeTimeAddition; } }


    [HideInInspector]
    public GameObject parentObject { get; set; }
    [HideInInspector]
    public WeaponController weaponController;
    public GameObject FireTransform;
    public Transform FirePoint { get{
        if (FireTransform == null) {
            // raise an error in debug console
            Debug.LogError("FireTransform is not set");
            return null;
        }
        if (FireTransform.transform == null) {
            // raise an error in debug console
            Debug.LogError("FireTransform.transform is not set");
            return null;
        }
        return FireTransform.transform;
    }
    }

    public void Shoot() {
        if (weaponController.m_Bullets == null) {
            weaponController.m_Bullets = new List<GameObject>();
        }
        switch (BulletType) {
            
            case ProjectileType.Bullet:
                Shoot_Bullet();
                break;
            case ProjectileType.Missile:
                Shoot_Missile();
                break;
            default:
                // raise an error in debug console
                Debug.LogError("Invalid ProjectileType");
                break;
        }
    }    

    private void Shoot_Bullet() {
        // spawn bullet at the FirePoint position and rotation with the parent object as local position and rotation
        var spawnPosition = FirePoint.position + parentObject.transform.position;
        var spawnRotation = parentObject.transform.rotation * FirePoint.rotation;
        GameObject bullet = Instantiate(BulletPrefab, spawnPosition, spawnRotation);
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        bullet.GetComponent<Projectile>().weapon = this;
        weaponController.m_Bullets.Add(bullet);
        // make the bullet don't collide with the parent object
        Physics.IgnoreCollision(bullet.GetComponent<Collider>(), parentObject.GetComponent<Collider>());
        // make the bullet don't collide with the other bullets
        foreach (var b in weaponController.m_Bullets) {
            Physics.IgnoreCollision(b.GetComponent<Collider>(), bullet.GetComponent<Collider>());
        }
        // make the bullet move forward
        // the direction is the forward direction of the FirePoint in quaternion multiplied by the rotation of the parent object
        var direction = parentObject.transform.rotation * FirePoint.forward;
        rb.AddForce(direction.normalized * velocity, ForceMode.VelocityChange);
    }

    private void Shoot_Missile() {
        var spawnPosition = FirePoint.position + parentObject.transform.position;
        var spawnRotation = parentObject.transform.rotation * FirePoint.rotation;
        GameObject missile = Instantiate(BulletPrefab, spawnPosition, spawnRotation);
        Rigidbody rb = missile.GetComponent<Rigidbody>();
        missile.GetComponent<Projectile>().weapon = this;
        weaponController.m_Bullets.Add(missile);
        // make the missile don't collide with the parent object
        Physics.IgnoreCollision(missile.GetComponent<Collider>(), parentObject.GetComponent<Collider>());
        // make the missile don't collide with the other bullets
        foreach (var b in weaponController.m_Bullets) {
            Physics.IgnoreCollision(b.GetComponent<Collider>(), missile.GetComponent<Collider>());
        }

        // make the missile move forward
        // the direction is the forward direction of the FirePoint in quaternion multiplied by the rotation of the parent object
        var direction = parentObject.transform.rotation * FirePoint.forward;
        rb.AddForce(direction.normalized * velocity, ForceMode.Impulse);
    }

    public void OnHit(GameObject bullet, GameObject target) {
        if (bullet == null) {
            return;
        }
        // if the target is not null, call the OnHit method of the target
        if (target != null && target.GetComponent<PlayObject>() != null){
            target.GetComponent<PlayObject>().OnHit(bullet);
        }

        // destroy the bullet
        weaponController.m_Bullets.Remove(bullet);
        Destroy(bullet);
    }
}

public enum ProjectileType {
    Bullet,
    Missile
}