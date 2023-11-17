using System;
using UnityEngine;

public class UVehicleController : MonoBehaviour {
    public UVehicleInstance vehicleInstance;

    public ForceComposite forceComposite {get{
        ForceComposite forces = new();

        // calculate forceComposite from main thrusters
        forces.front = 0;
        forces.back = 0;
        forces.leftRight = 0;
        forces.upDown = 0;
        forces.pitch = 0;
        forces.yaw = 0;
        forces.roll = 0;
        var thrusters = vehicleInstance.vehicleType.vehicleLayout.thrusters;
        foreach(var thruster in thrusters){
            if(thruster.isDisabled){
                continue;
            }

            var thrusterPosition = thruster.thrusterPosition;
            var thrusterForce = thruster.thrusterForce;
            var thrusterDirection = thruster.thrusterDirection;

            // TO:DO
        }
        
        
        // set pitch, yaw, roll to gyro values if useGyro is true
        if(useGyro){
            var gyroValue = vehicleInstance.vehicleType.gyroValue;
            forces.pitch = gyroValue.pitch;
            forces.yaw = gyroValue.yaw;
            forces.roll = gyroValue.roll;
        }

        return forces;
    }}

    internal bool useGyro {get{
        return vehicleInstance.vehicleType.useGyro;
    }}
}

public struct ForceComposite
{
    public float front;
    public float back;
    public float leftRight;
    public float upDown;
    public float pitch;
    public float yaw;
    public float roll;
}