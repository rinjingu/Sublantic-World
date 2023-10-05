using UnityEngine;

[System.Serializable]
public sealed class ComponentInstance
{
    public ShipComponent componentType;

    public ComponentInstance(ShipComponent componentType)
    {
        this.componentType = componentType;
    }
}
