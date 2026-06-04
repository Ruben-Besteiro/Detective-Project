using System.Collections;
using UnityEngine;

public class PickupInteractable : Interactable
{
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        promptText.text = "Pulsa E para obtener " + displayName;
    }

    public override void OnInteract()
    {
        StartCoroutine(IE_Pickup());
    }

    private IEnumerator IE_Pickup()
    {
        promptText.text = "Has obtenido " + displayName;
        // "Destruimos" el objeto
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        yield return new WaitForSeconds(1);

        // TO DO: Añadir al inventario
        promptText.text = "";
        Destroy(gameObject);
    }
}
