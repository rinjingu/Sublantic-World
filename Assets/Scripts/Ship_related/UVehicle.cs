using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UVehicle", menuName = "Sublantic World/UVehicle", order = 0)]
public sealed class UVehicle : ScriptableObject
{
    public List<ComponentInstance> components = new List<ComponentInstance>();
}
