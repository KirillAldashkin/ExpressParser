using System.Collections;

namespace ExpressParser;

/// <summary>
/// A dictionary with zero-cost access to an array of values.
/// </summary>
class ArrayDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
{
    Dictionary<TKey, int> dict;
    public TValue[] ValuesArray;

    public ArrayDictionary(ArrayDictionary<TKey, TValue> arguments)
    {
        dict = new(arguments.dict);
        ValuesArray = new TValue[arguments.Count];
        Array.Copy(arguments.ValuesArray, ValuesArray, arguments.Count);
    }

    public ArrayDictionary()
    {
        dict = new();
        ValuesArray = new TValue[0];
    }

    public IEnumerable<TKey> Keys => dict.Keys;
    public IEnumerable<TValue> Values => ValuesArray;
    public int Count => ValuesArray.Length;

    public TValue this[TKey key]
    {
        get => ValuesArray[dict[key]];
        set => ValuesArray[dict[key]] = value;
    }
    public void Add(TKey key, TValue value)
    {
        dict[key] = ValuesArray.Length;
        var oldArray = ValuesArray;
        ValuesArray = new TValue[ValuesArray.Length + 1];
        Array.Copy(oldArray, ValuesArray, oldArray.Length);
        ValuesArray[ValuesArray.Length - 1] = value;
    }

    public bool ContainsKey(TKey key) => dict.ContainsKey(key);

    public bool TryGetValue(TKey key, out TValue value)
    {
        if (!dict.TryGetValue(key, out int index))
        {
            value = default;
            return false;
        }
        value = ValuesArray[index];
        return true;
    }

    /// <summary>
    /// Not implemented.
    /// </summary>
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => throw new NotImplementedException();

    /// <summary>
    /// Not implemented.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
}