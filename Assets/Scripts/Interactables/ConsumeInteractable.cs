using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsumeInteractable : Interactable
{
    [SerializeField] List<ItemData> acceptedItems;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        SetPrompt("Pulsa E para usar " + displayName);
    }

    public override void OnInteract()
    {
        var inventory = GameDataManager.Instance.inventory;

        foreach (var item in acceptedItems)
        {
            int index = inventory.FindIndex(d => d.item == item);
            if (index < 0) continue;

            var entry = inventory[index];
            entry.quantity--;
            if (entry.quantity <= 0)
                inventory.RemoveAt(index);
            else
                inventory[index] = entry;

            StartCoroutine(IE_Consume(item.itemName));
            return;
        }

        StartCoroutine(IE_NoItem());
    }

    private IEnumerator IE_Consume(string itemName)
    {
        promptText.text = "Has usado " + itemName;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(1);

        Destroy(gameObject);
    }

    private IEnumerator IE_NoItem()
    {
        promptText.text = "No tienes lo que se necesita";
        yield return new WaitForSeconds(1);
        promptText.text = "Pulsa E para usar " + displayName;
    }
}
