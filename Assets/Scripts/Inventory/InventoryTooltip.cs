using TMPro;
using UnityEngine;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance;

    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text description;
    [SerializeField] private TMP_Text quantity;

    [SerializeField]
    private Vector2 offset = new Vector2(20f, -20f);

    private RectTransform rectTransform;

    private void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start()
    {
        Hide();
    }

    private void Update()
    {
        rectTransform.position =
            Input.mousePosition + (Vector3)offset;
    }

    public void Show(PickupData data)
    {
        gameObject.SetActive(true);

        itemName.text = data.item.itemName;
        description.text = data.item.description;
        quantity.text = $"x{data.quantity}";
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}