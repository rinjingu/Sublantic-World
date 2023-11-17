using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UVehicle", menuName = "Sublantic World/UVehicle", order = 0)]
public sealed class UVehicle : ScriptableObject
{
    public string vehicleName;
    public string vehicleDescription;
    public Sprite vehicleIcon;
    public GameObject vehiclePrefab;

    public bool useGyro { get{
        if(gyroValue.pitch == 0 && gyroValue.yaw == 0 && gyroValue.roll == 0){
            return false;
        }else{
            return true;
        }
    }}
    
    public List<ComponentInstance> components;

    public VehicleLayout vehicleLayout;

    public int TurretSlotCount
    {
        get { return vehicleLayout.turretSlots.Count; }
    }

    public GyroValue gyroValue;
}

[Serializable]
public struct GyroValue
{
    public float pitch;
    public float yaw;
    public float roll;
}
