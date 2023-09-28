using UnityEngine;

[CreateAssetMenu(fileName = "ShipComponent", menuName = "Sublantic World/ShipComponent", order = 0)]
public class ShipComponent : ScriptableObject {
    public Sprite icon;
    public string componentName;
    [TextArea]
    public string desc;
    public int componentID;
}