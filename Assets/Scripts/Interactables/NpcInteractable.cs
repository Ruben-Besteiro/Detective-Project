using UnityEngine;

public class NpcInteractable : Interactable
{
    [SerializeField] private DialogueData dialogue;

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        SetPrompt("Pulsa E para hablar con " + displayName);
    }

    public override void OnInteract()
    {
        if (dialogue != null)
            DialogueManager.StartDialogue(dialogue);
    }
}
