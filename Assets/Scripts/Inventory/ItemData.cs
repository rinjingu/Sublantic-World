using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemData", menuName = "Sublantic World/ItemData", order = 0)]
public class ItemData : ScriptableObject
{
    public Sprite icon;
    public string componentName;

    [TextArea]
    public string desc;
    [ReadOnly]
    public string ID = Guid.NewGuid().ToString();
    public int maxCount;

    public List<SerializableKeyValuePair> attribute = new List<SerializableKeyValuePair>();

    public Dictionary<string, string> GetAttributes()
    {
        Dictionary<string, string> dict = new();
        foreach (SerializableKeyValuePair kvp in attribute)
        {
            dict.Add(kvp.Key, kvp.Value);
        }
        return dict;
    }

    public bool IsStackable()
    {
        return maxCount > 1;
    }

}
