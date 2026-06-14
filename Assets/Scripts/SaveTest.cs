using UnityEngine;

public class SaveTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            GameDataManager.Instance.currentHypothesis = Hypotheses.H1;
            FlagManager.Instance.SetFlag("test_flag", true);
            GameDataManager.Instance.TrySave();
            Debug.Log("[Test] Guardado");
        }

        if (Input.GetKeyDown(KeyCode.F6))
        {
            Debug.Log($"[Test] MaxHp: {GameDataManager.Instance.PlayerMaxHp}");
            Debug.Log($"[Test] Hipótesis: {GameDataManager.Instance.currentHypothesis}");
            Debug.Log($"[Test] Flag test_flag: {FlagManager.Instance.HasFlag("test_flag")}");
            Debug.Log($"[Test] Items en inventario: {GameDataManager.Instance.inventory.Count}");
            foreach (var pickup in GameDataManager.Instance.inventory)
                Debug.Log($"  - {pickup.item.itemName} x{pickup.quantity}");
        }
    }
}
