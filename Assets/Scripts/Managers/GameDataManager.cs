using UnityEngine;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour, ISaveable
{
    public static GameDataManager Instance;

    public List<PickupData> inventory = new();
    public Hypotheses currentHypothesis = Hypotheses.None;

    [SerializeField] private ItemData[] itemDatabase;
    [SerializeField] private PlayerStats playerStats;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveData(SaveData data)
    {
        data.currentHypothesis = (int)currentHypothesis;
        data.playerMaxHp = playerStats.maxHp;

        data.inventoryItemNames = new List<string>();
        data.inventoryQuantities = new List<int>();
        foreach (var pickup in inventory)
        {
            data.inventoryItemNames.Add(pickup.item != null ? pickup.item.name : "");
            data.inventoryQuantities.Add(pickup.quantity);
        }
    }

    public void LoadData(SaveData data)
    {
        currentHypothesis = (Hypotheses)data.currentHypothesis;
        if (data.playerMaxHp > 0) playerStats.maxHp = data.playerMaxHp;
        inventory = new List<PickupData>();

        if (data.inventoryItemNames == null) return;

        for (int i = 0; i < data.inventoryItemNames.Count; i++)
        {
            ItemData item = FindItemByName(data.inventoryItemNames[i]);
            if (item != null)
                inventory.Add(new PickupData { item = item, quantity = data.inventoryQuantities[i] });
        }
    }

    private ItemData FindItemByName(string itemName)
    {
        foreach (var item in itemDatabase)
        {
            if (item != null && item.name == itemName)
                return item;
        }
        return null;
    }

    public float PlayerMaxHp => playerStats.maxHp;

    public void IncreaseMaxHP(float amount)
    {
        playerStats.maxHp += amount;
    }
}

[System.Serializable]
public struct PickupData
{
    public ItemData item;
    public int quantity;
}

public enum Hypotheses
{
    H1, H2, H3, None
}
