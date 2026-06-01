using System.Collections.Generic;
using UnityEngine;

public class FlagManager : MonoBehaviour
{
    public static FlagManager Instance { get; private set; }

    private Dictionary<string, bool> flags = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool GetFlag(string flagName)
    {
        return flags.TryGetValue(flagName, out bool value) && value;
    }

    public void SetFlag(string flagName, bool value)
    {
        flags[flagName] = value;
    }
}