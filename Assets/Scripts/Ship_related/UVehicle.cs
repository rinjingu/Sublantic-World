using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "UVehicle", menuName = "Sublantic World/UVehicle", order = 0)]
public sealed class UVehicle : ScriptableObject
{
    public string vehicleName;
    public string vehicleDescription;
    public Sprite vehicleIcon;
    public GameObject vehiclePrefab;

    [ReadOnly]
    public string vehicleID = Guid.NewGuid().ToString();
    public List<ComponentInstance> components = new();

    public VehicleLayout vehicleLayout = new();
}

public struct BaseAttributes { }
