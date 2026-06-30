using UnityEngine;
using System.Collections;

public class SaveTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            GameDataManager.Instance.IncreaseMaxHP(999999);
            Debug.Log("[TestHpBoost] MaxHp aumentado. Nuevo valor: " + GameDataManager.Instance.playerStats.maxHp);
        }

        if (Input.GetKeyDown(KeyCode.F5))
        {
            GameDataManager.Instance.currentHypothesis = Hypotheses.H1;
            FlagManager.Instance.SetFlag("test_flag", true);
            SaveManager.Instance.TrySave();
            Debug.Log("[Test] Guardado");
        }

        if (Input.GetKeyDown(KeyCode.F6))
            StartCoroutine(IE_LoadAndLog());
    }

    private IEnumerator IE_LoadAndLog()
    {
        SaveManager.Instance.TryLoadAndApply();
        yield return null;      // Esto es necesario para que aparezcan las cosas correctas
        Debug.Log("[Test] MaxHp: " + GameDataManager.Instance.playerStats.maxHp);
        Debug.Log("[Test] Hipótesis: " + GameDataManager.Instance.currentHypothesis);
        Debug.Log("[Test] Flag test_flag: " + FlagManager.Instance.HasFlag("test_flag"));
        Debug.Log("[Test] Items en inventario: " + GameDataManager.Instance.inventory.Count);
        foreach (var pickup in GameDataManager.Instance.inventory)
            Debug.Log("  - " + pickup.item.itemName + " x" + pickup.quantity);
    }
}
