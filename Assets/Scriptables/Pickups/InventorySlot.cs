using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text quantityText;

    public PickupData Data { get; private set; }

    public void Setup(PickupData data)
    {
        Data = data;

        icon.sprite = data.item.image;
        quantityText.text = data.quantity.ToString();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        InventoryTooltip.Instance.Show(Data);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventoryTooltip.Instance.Hide();
    }
}