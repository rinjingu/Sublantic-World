using System;
using Unity.Collections;

[Serializable]
public sealed class UVehicleInstance{
    public UVehicle vehicleType;

    [ReadOnly]
    public string vehicleID = Guid.NewGuid().ToString();

    public string vehicleName;
}