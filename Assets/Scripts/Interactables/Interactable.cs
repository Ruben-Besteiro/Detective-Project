using System;
using TMPro;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public static event Action<Interactable> OnEnterRange;
    public static event Action<Interactable> OnExitRange;

    [SerializeField] protected string displayMessage;
    [SerializeField] protected GameObject interactCanvas;
    [SerializeField] protected DialogueData dialogue;
    protected TMP_Text promptText;
    private string savedPrompt;

    protected virtual void Start()
    {
        promptText = interactCanvas.GetComponentInChildren<TMP_Text>();
    }

    protected void SetPrompt(string text)
    {
        savedPrompt = text;
        promptText.text = text;
    }

    public void OnInteract()
    {
        SetPromptActive(false);
        if (dialogue != null && !ShouldSkipDialogue())
        {
            DialogueManager.OnDialogueFinished += HandleDialogueFinished;
            DialogueManager.StartDialogue(dialogue);
        }
        else
            DoThing();
    }

    // Hacemos que esta función se pueda sobreescribir en las clases hijas
    // Esto de momento solo es true en los objetos Consume cuando tenemos lo que se pide
    protected virtual bool ShouldSkipDialogue()
        => false;

    private void HandleDialogueFinished()
    {
        DialogueManager.OnDialogueFinished -= HandleDialogueFinished;
        DoThing();
    }

    protected virtual void DoThing() { }

    // Activamos o desactivamos el texto de que puedes interactuar con el objeto
    public void SetPromptActive(bool active)
    {
        if (interactCanvas != null)
            promptText.text = active ? savedPrompt : "";
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnEnterRange?.Invoke(this);     // Añade el objeto a la lista de objetos en los que puedes interactuar
        SetPrompt("Pulsa E para " + displayMessage);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnExitRange?.Invoke(this);
    }
}