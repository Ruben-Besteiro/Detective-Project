using System.Collections;
using UnityEngine;

public class PickupInteractable : Interactable
{
    [SerializeField] ItemData itemData;
    public ItemData ItemData => itemData;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        SetPrompt("Pulsa E para obtener " + itemData.itemName);
    }

    public override void OnInteract()
    {
        StartCoroutine(IE_Pickup());
    }

    private IEnumerator IE_Pickup()
    {
        promptText.text = "";
        FeedbackAnimationManager.Instance.PlayPickupAnimation(itemData.image, itemData.name);
        // "Destruimos" el objeto
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        // Si el objeto no está en el inventario, lo añadimos
        // Pero si sí está, aumentamos su cantidad en 1
        var inventory = GameDataManager.Instance.inventory;
        int index = inventory.FindIndex(d => d.item == itemData);
        if (index >= 0)
        {
            var entry = inventory[index];
            entry.quantity++;
            inventory[index] = entry;
        }
        else
            inventory.Add(new PickupData { item = itemData, quantity = 1 });

        yield return new WaitForSeconds(1);

        promptText.text = "";
        Destroy(gameObject);
    }
}
