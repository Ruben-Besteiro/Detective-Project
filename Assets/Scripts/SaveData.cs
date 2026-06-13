using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentHypothesis;
    public List<string> inventoryItemNames;
    public List<int> inventoryQuantities;
    public List<string> flagKeys;
    public List<bool> flagValues;

    public SaveData()
    {
        currentHypothesis = (int)Hypotheses.None;
        inventoryItemNames = new List<string>();
        inventoryQuantities = new List<int>();
        flagKeys = new List<string>();
        flagValues = new List<bool>();
    }
}
