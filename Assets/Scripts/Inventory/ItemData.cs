using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Sublantic World/ItemData", order = 0)]
public class ItemData : ScriptableObject {
    public Sprite icon;
    public string componentName;
    [TextArea]
    public string desc;
    public int itemID;
    public int maxCount;
}