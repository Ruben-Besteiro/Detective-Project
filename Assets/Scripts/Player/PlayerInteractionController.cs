using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractionController : MonoBehaviour
{
    public static PlayerInteractionController Instance;

    InputActions input;
    List<Interactable> interactablesInRange = new();
    Interactable currentInteractableObject;
    public LinkableInteractable currentlySelectedLinkable;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        input = new InputActions();
    }

    void OnEnable()
    {
        input.Player.Interact.Enable();
        input.Player.Interact.performed += OnInteract;
        Interactable.OnEnterRange += OnEnterRange;
        Interactable.OnExitRange  += OnExitRange;
        DialogueManager.OnDialogueStarted  += DisableInteract;
        DialogueManager.OnDialogueFinished += EnableInteract;
        PauseController.OnPauseStarted += DisableInteract;
        PauseController.OnPauseEnded   += EnableInteract;
    }

    void OnDisable()
    {
        input.Player.Interact.Disable();
        input.Player.Interact.performed -= OnInteract;
        Interactable.OnEnterRange -= OnEnterRange;
        Interactable.OnExitRange  -= OnExitRange;
        DialogueManager.OnDialogueStarted  -= DisableInteract;
        DialogueManager.OnDialogueFinished -= EnableInteract;
        PauseController.OnPauseStarted -= DisableInteract;
        PauseController.OnPauseEnded   -= EnableInteract;
    }

    void DisableInteract() => input.Player.Interact.Disable();
    void EnableInteract()  => input.Player.Interact.Enable();

    void OnEnterRange(Interactable interactable) => interactablesInRange.Add(interactable);

    void OnExitRange(Interactable interactable)
    {
        interactablesInRange.Remove(interactable);
        interactable.SetPromptActive(false);
        if (currentInteractableObject == interactable)
            currentInteractableObject = null;
    }

    void OnInteract(InputAction.CallbackContext ctx) => currentInteractableObject?.OnInteract();

    void Update()
    {
        UpdateCurrentInteractable();
    }

    void UpdateCurrentInteractable()
    {
        interactablesInRange.RemoveAll(i => i == null);     // Quitamos objetos destruidos

        // Si hay varios en rango, pillamos el más cercano
        Interactable closest = null;

        if (interactablesInRange.Count > 0)
        {
            float minDist = float.MaxValue;
            foreach (var interactable in interactablesInRange)
            {
                float dist = Vector3.SqrMagnitude(transform.position - interactable.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    closest = interactable;
                }
            }
        }

        if (closest == currentInteractableObject) return;

        currentInteractableObject?.SetPromptActive(false);
        currentInteractableObject = closest;
        currentInteractableObject?.SetPromptActive(true);
    }
}
