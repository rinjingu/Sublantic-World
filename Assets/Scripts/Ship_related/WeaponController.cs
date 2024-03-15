using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class WeaponController : MonoBehaviour {
    public Weapon weapon;
    private GameObject m_GameObject;
    public GameObject Target;

    [SerializeField]
    public bool isFiring;
    public List<GameObject> m_Bullets;

    private void Start() {
        weapon.FireCooldown = 0f;
        isFiring = false;
        m_GameObject = gameObject;
        weapon.parentObject = m_GameObject;
        weapon.weaponController = this;
        m_Bullets = new List<GameObject>();
    }

    private void Update() {
        // if (isFiring) {
        //     if (weapon.FireCooldown <= 0) {
        //         Shoot();
        //         weapon.FireCooldown = 1f / weapon.FireRate;
        //     }
        //     weapon.FireCooldown -= Time.deltaTime;
        // }
        if (isFiring && weapon.FireCooldown <= 0) {
            
            try
            {
                Shoot();
            }
            finally
            {
                weapon.FireCooldown = 1f / weapon.FireRate;
            }
            
        }

        if (weapon.FireCooldown > 0) {
            weapon.FireCooldown -= Time.deltaTime;
        }

        
        for (int i = 0; i < m_Bullets.Count; i++) {
            // check if the bullets are out of range
            if (Vector3.Distance(m_Bullets[i].transform.position, m_GameObject.transform.position) > weapon.MaxRange) {
                Destroy(m_Bullets[i]);
                m_Bullets.RemoveAt(i);
            }
        }
    }

    private void Shoot() {
        weapon.Shoot();
    }

}