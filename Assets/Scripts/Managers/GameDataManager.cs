using UnityEngine;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance;

    public List<PickupData> inventory = new();
    public Hypotheses currentHypothesis = Hypotheses.None;

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
}

[System.Serializable]
public struct PickupData
{
    public ItemData item;
    public int quantity;
}

public enum Hypotheses
{
    // TO DO: Ponerles nombre o algo
    H1, H2, H3, None
}