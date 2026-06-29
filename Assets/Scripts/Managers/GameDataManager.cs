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
        data.playerSpeed = playerStats.speed;
        data.playerDashSpeed = playerStats.dashSpeed;
        data.playerDashDuration = playerStats.dashDuration;
        data.playerDashCooldown = playerStats.dashCooldown;
        data.playerDodgeSpeed = playerStats.dodgeSpeed;
        data.playerDodgeDuration = playerStats.dodgeDuration;
        data.playerDodgeCooldown = playerStats.dodgeCooldown;

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
        if (data.playerSpeed > 0) playerStats.speed = data.playerSpeed;
        if (data.playerDashSpeed > 0) playerStats.dashSpeed = data.playerDashSpeed;
        if (data.playerDashDuration > 0) playerStats.dashDuration = data.playerDashDuration;
        if (data.playerDashCooldown > 0) playerStats.dashCooldown = data.playerDashCooldown;
        if (data.playerDodgeSpeed > 0) playerStats.dodgeSpeed = data.playerDodgeSpeed;
        if (data.playerDodgeDuration > 0) playerStats.dodgeDuration = data.playerDodgeDuration;
        if (data.playerDodgeCooldown > 0) playerStats.dodgeCooldown = data.playerDodgeCooldown;
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
