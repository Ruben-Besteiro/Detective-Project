using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HypothesisVersion
{
    public string title;

    [TextArea(3, 10)]
    public string description;

    public List<string> requiredFlags;
}