using System;
using Unity.Mathematics;
using UnityEngine;

public class Projectile : MonoBehaviour {
    public bool isDestroyable = false;
    public bool isMovable = false;
    [HideInInspector]
    public Weapon weapon;

    [SerializeField]
    private float m_ExistTime = 0f;
    [SerializeField]
    private float m_TraveledDistance = 0f;
    private void OnCollisionEnter(Collision other) {
        weapon.OnHit(gameObject, other.gameObject);
    }

    private void Update() {
        m_ExistTime += Time.deltaTime;
        m_TraveledDistance += Time.deltaTime * GetComponent<Rigidbody>().velocity.magnitude;
        switch (weapon.BulletType)
        {
            case ProjectileType.Missile:
                isMovable = m_ExistTime > weapon.MissileLaunchTime ? true : false;
                MissileTracking();
                if (m_ExistTime > weapon.MaxLifeTime){weapon.OnHit(gameObject, null);}
                break;
            // Add more cases for other bullet types if needed
            case ProjectileType.Bullet:
                BulletMotion();
                break;
            default:
                break;
        }
    }

    private void BulletMotion(){
        Rigidbody rb = GetComponent<Rigidbody>();
        Transform transform = GetComponent<Transform>();

        // the speed of the bullet follows a specific equation
        var p_d = m_TraveledDistance / weapon.MaxRange;
        var gain = 0.314f;
        var shift = 0.7f;
        var constant = 1f;
        var multiply = 16.4f;
        // r = c - a * (2 / (1 + e^(-k * (x - s))))
        var rate = constant - gain * (2 / (1 + math.exp(-multiply *(p_d - shift))));
        // y = r * (1 - l * x), preform a system rotation to rate 
        var lerp = 0.1f;
        rate = rate * (1 - lerp * p_d);
        rate = Mathf.Sqrt(rate);
        rb.velocity = transform.forward * weapon.velocity * rate;
    }

    private void MissileTracking(){
        Rigidbody rb = GetComponent<Rigidbody>();
        Transform transform = GetComponent<Transform>();
        if (weapon.weaponController.Target == null || isMovable == false) {
            var direction = transform.forward;
            // add a speed up effect powered by a sigmoid function
            var rate = (float)math.min(1, 2/(1+math.exp(-(m_ExistTime/weapon.MissileLaunchTime)))-0.45);
            rb.velocity = direction.normalized * weapon.velocity * rate;
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

        // calculate the velocity to the target
        var velocity = transform.forward * weapon.velocity;
        rb.velocity = velocity;
    }
}