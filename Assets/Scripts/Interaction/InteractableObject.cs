using System;
using TMPro;
using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public static event Action<InteractableObject> OnFocusChanged;

    [SerializeField] DialogueData dialogue;
    [SerializeField] string thingName;
    [SerializeField] GameObject interactCanvas;

    TMP_Text promptText;

    void Start()
    {
        if (interactCanvas != null)
            promptText = interactCanvas.GetComponentInChildren<TMP_Text>();

        interactCanvas?.SetActive(false);
    }

    public void Interact()
    {
        if (dialogue != null)
            DialogueManager.StartDialogue(dialogue);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnFocusChanged?.Invoke(this);
        if (promptText != null)
            promptText.text = "Pulsa E para interactuar con " + thingName;
        interactCanvas?.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnFocusChanged?.Invoke(null);
        interactCanvas?.SetActive(false);
    }
}
