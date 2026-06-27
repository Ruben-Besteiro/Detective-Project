using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance;

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

    public void TrySave()
    {
        if (data == null)
            data = new SaveData();

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

    public void TryLoadAndApply()
    {
        data = TryLoad();
        StartCoroutine(IE_LoadEveryISaveable());
    }

    private SaveData TryLoad()
    {
        if (!File.Exists(savePath))
            return new SaveData();

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

    private IEnumerator IE_LoadEveryISaveable()
    {
        yield return null;

        foreach (MonoBehaviour mono in FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None))
        {
            if (mono is ISaveable saveable)
                saveable.LoadData(data);
        }
    }
}
