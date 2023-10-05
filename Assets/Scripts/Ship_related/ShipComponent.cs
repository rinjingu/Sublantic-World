using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShipComponent", menuName = "Sublantic World/ShipComponent", order = 0)]
public class ShipComponent : ScriptableObject
{
    public Sprite icon
    {
        get { return this.prefab.GetComponent<SpriteRenderer>().sprite; }
    }
    public GameObject prefab;
    public string componentName;

    [TextArea]
    public string desc;
    public int componentID;

    [SerializeField]
    private List<SerializableKeyValuePair> attribute = new List<SerializableKeyValuePair>();

    public Dictionary<string, string> GetAttributes()
    {
        Dictionary<string, string> dict = new();
        foreach (SerializableKeyValuePair kvp in attribute)
        {
            dict.Add(kvp.Key, kvp.Value);
        }
        return dict;
    }
}
