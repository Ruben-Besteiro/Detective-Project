using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private Transform gridParent;
    [SerializeField] private InventorySlot slotPrefab;

    private readonly List<InventorySlot> slots = new();

    private void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        foreach (var slot in slots)
            Destroy(slot.gameObject);

        slots.Clear();

        foreach (var item in GameDataManager.Instance.inventory)
        {
            InventorySlot slot =
                Instantiate(slotPrefab, gridParent);

            slot.Setup(item);

            slots.Add(slot);
        }
    }
}