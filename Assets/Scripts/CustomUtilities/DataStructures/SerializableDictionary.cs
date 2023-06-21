using System.Collections.Generic;

[System.Serializable]
public class KeyValueEntry<K, V>
{
    public K Key;
    public V Value;

    public KeyValueEntry(K key, V value)
    {
        Key = key;
        Value = value;
    }
}

[System.Serializable]
public class SerializableDictionary<K, V>
{
    public static implicit operator Dictionary<K, V>(SerializableDictionary<K, V> d) => d.ToDictionary();

    public List<KeyValueEntry<K, V>> Dict;

    public SerializableDictionary()
    {
        Dict = new List<KeyValueEntry<K, V>>();
    }

    public void Add(K key, V value)
    {
        if (KeyExists(key))
        {
            Remove(key);
        }

        KeyValueEntry<K, V> newEntry = new KeyValueEntry<K, V>(key, value);
        Dict.Add(newEntry);
    }

    public bool Get(K key, ref V value)
    {
        foreach (KeyValueEntry<K, V> entry in Dict)
        {
            if (KeysAreEquals(entry.Key, key))
            {
                value = entry.Value;
                return true;
            }
        }

        return false;
    }

    public void Remove(K key)
    {
        Dict.RemoveAll(entry => KeysAreEquals(entry.Key, key));
    }

    public bool KeyExists(K key)
    {
        foreach (KeyValueEntry<K, V> entry in Dict)
        {
            if (KeysAreEquals(entry.Key, key))
            {
                return true;
            }
        }

        return false;
    }

    private bool KeysAreEquals(K key1, K key2)
    {
        if (key1 == null)
        {
            if (key2 == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return key1.Equals(key2);
        }
    }

    private Dictionary<K, V> ToDictionary()
    {
        Dictionary<K, V> dictionary = new Dictionary<K, V>();

        foreach (KeyValueEntry<K, V> entry in Dict)
        {
            dictionary.Add(entry.Key, entry.Value);
        }

        return dictionary;
    }
}
