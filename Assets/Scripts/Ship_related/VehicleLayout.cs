using System.Collections.Generic;
using UnityEngine;

public class VehicleLayout
{
    public List<Propeller> propellers = new List<Propeller>();
}

public struct Propeller
{
    public float maxForce;
    public float relativePosition;
}

public struct TurretSlot
{
    public string SlotSize;
    public int SlotId;
}
