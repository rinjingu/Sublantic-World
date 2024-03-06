using System;
using Unity.Mathematics;
using UnityEngine;

public class UVehicleController : MonoBehaviour {
    public UVehicleInstance vehicleInstance;

    public ForceComposite forceComposite {get{
        ForceComposite forces = new();

        // calculate forceComposite from main thrusters
        forces.velocity = Vector3.zero;
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

            // TODO: calculate forceComposite from thrusters
            // calculate distance from thruster to center of mass
            var distance = math.distance(thrusterPosition, Vector3.zero);
            // get the angle between thruster and center of mass
            var angle = Vector3.Angle(thrusterPosition, Vector3.zero);
            
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
    public Vector3 velocity;
    public float pitch;
    public float yaw;
    public float roll;
}