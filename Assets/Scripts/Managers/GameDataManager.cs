using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.SceneManagement;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public List<PickupData> inventory = new();
    public Hypotheses currentHypothesis = Hypotheses.None;

    [SerializeField] private ItemData[] itemDatabase;
    [SerializeField] private PlayerStats playerStats;

    private string savePath;
    private SaveData data;

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
            return;
        }

        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    void OnEnable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Cuando abrimos el juego, cargamos la partida
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si no existe lo creamos
        if (data == null)
        {
            data = TryLoad();
            LoadDataFromGDM(data);
        }

        StartCoroutine(IE_LoadEveryISaveable());
    }

    IEnumerator IE_LoadEveryISaveable()
    {
        yield return null;      // Esperamos 1 frame porque si no no va

        // Recorremos todos los scripts y llamamos al LoadData si lo tienen
        foreach (MonoBehaviour mono in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mono is ISaveable saveable)
                saveable.LoadData(data);
        }
    }

    public void TrySave()
    {
        if (data == null)
            data = new SaveData();

        SaveDataFromGDM(data);

        foreach (MonoBehaviour mono in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mono is ISaveable saveable)
                saveable.SaveData(data);
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(savePath, json);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al guardar la partida: " + e.Message);
        }
    }

    private SaveData TryLoad()
    {
        if (!File.Exists(savePath))
            return new SaveData();

        // Leemos el archivo y lo convertimos en SaveData
        try
        {
            string json = File.ReadAllText(savePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch (Exception e)
        {
            Debug.LogError("Error al cargar la partida: " + e.Message);
            return new SaveData();
        }
    }

    private void SaveDataFromGDM(SaveData data)
    {
        data.currentHypothesis = (int)currentHypothesis;

        // Stats del jugador
        data.playerMaxHp = playerStats.maxHp;

        // Inventario
        data.inventoryItemNames = new List<string>();
        data.inventoryQuantities = new List<int>();

        foreach (var pickup in inventory)
        {
            data.inventoryItemNames.Add(pickup.item != null ? pickup.item.name : "");
            data.inventoryQuantities.Add(pickup.quantity);
        }
    }

    private void LoadDataFromGDM(SaveData data)
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

    // -------------------------------------------------

    public float PlayerMaxHp => playerStats.maxHp;

    // En la escena de investigación se pueden subir stats, y esos cambios se aplican en la escena de combate
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
