using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LinkableInteractable : Interactable
{
    [SerializeField] private List<LinkableInteractable> compatibleObjects;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        promptText.text = "Pulsa E para vincular " + displayName;
    }

    public override void OnInteract()
    {
        LinkableInteractable selected = PlayerInteractionController.Instance.currentlySelectedLinkable;

        if (selected == null)
        {
            StartCoroutine(IE_PickLinkable("Has seleccionado el vinculable " + displayName));
            PlayerInteractionController.Instance.currentlySelectedLinkable = this;
            return;
        }

        if (selected == this)
        {
            StartCoroutine(IE_PickLinkable("No puedes vincular un objeto consigo mismo"));
            PlayerInteractionController.Instance.currentlySelectedLinkable = null;
            return;
        }

        if (selected.compatibleObjects.Contains(this))
            StartCoroutine(IE_PickLinkable("Has vinculado " + selected.displayName + " con " + displayName));
        else
            StartCoroutine(IE_PickLinkable("No hay correlación"));

        PlayerInteractionController.Instance.currentlySelectedLinkable = null;
    }

    private IEnumerator IE_PickLinkable(string message)
    {
        promptText.text = message;
        
        yield return new WaitForSeconds(1);
        promptText.text = "";
    }
}
