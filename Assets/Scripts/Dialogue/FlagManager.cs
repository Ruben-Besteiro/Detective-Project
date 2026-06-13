using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour, ISaveable
{
    public static FlagManager Instance { get; private set; }

    private readonly Dictionary<string, bool> flags = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool HasFlag(string flagName)
    {
        return flags.TryGetValue(flagName, out bool value) && value;
    }

    public void SetFlag(string flagName, bool value)
    {
        flags[flagName] = value;

        Debug.Log($"[Flag] {flagName} = {value}");
    }

    public void SaveData(SaveData data)
    {
        data.flagKeys = new List<string>();
        data.flagValues = new List<bool>();

        foreach (var kvp in flags)
        {
            data.flagKeys.Add(kvp.Key);
            data.flagValues.Add(kvp.Value);
        }
    }

    public void LoadData(SaveData data)
    {
        flags.Clear();

        if (data.flagKeys == null) return;

        for (int i = 0; i < data.flagKeys.Count; i++)
            flags[data.flagKeys[i]] = data.flagValues[i];
    }
}
