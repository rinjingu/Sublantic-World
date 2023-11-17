using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class VehicleLayout
{
    public List<TurretSlot> turretSlots = new();
    public List<Sector> sectors = new();

    public List<Thruster> thrusters = new();

}

[Serializable]
public struct TurretSlot
{
    public SlotSize slotSize;
    public int slotId;
}

[Serializable]
public struct Sector
{
    public int sectorId;
    public string sectorName;
    public Vector3 sectorPosition;
    public int sectorDurability;
}

[Serializable]
public struct Thruster 
{
    public string thrusterName;
    public Vector3 thrusterPosition;
    public Vector3 thrusterDirection;
    public float thrusterForce;
    public bool isDisabled;
    public float backPower;
}

[Serializable]
public enum SlotSize
{
    Small,
    Medium,
    Large
}