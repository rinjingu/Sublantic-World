[System.Serializable]
public class SerializableKeyValuePair
{
    public string Key;
    public string Value;

    public SerializableKeyValuePair(string key, string value)
    {
        Key = key;
        Value = value;
    }
}