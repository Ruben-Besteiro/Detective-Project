using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInvestigationController : PlayerController
{
    public static PlayerInvestigationController Instance;

    List<Interactable> interactablesInRange = new();
    Interactable currentInteractableObject;
    public LinkableInteractable currentlySelectedLinkable;

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        base.Awake();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        input.Player.Interact.Enable();
        input.Player.Interact.performed += OnInteract;
        Interactable.OnEnterRange += OnEnterRange;
        Interactable.OnExitRange  += OnExitRange;
        DialogueManager.OnDialogueStarted  += DisableInteract;
        DialogueManager.OnDialogueFinished += EnableInteract;
        PauseController.OnPauseStarted += DisableInteract;
        PauseController.OnPauseEnded   += EnableInteract;
    }

    protected override void OnDisable()
    {
        input.Player.Interact.performed -= OnInteract;
        input.Player.Interact.Disable();
        Interactable.OnEnterRange -= OnEnterRange;
        Interactable.OnExitRange  -= OnExitRange;
        DialogueManager.OnDialogueStarted  -= DisableInteract;
        DialogueManager.OnDialogueFinished -= EnableInteract;
        PauseController.OnPauseStarted -= DisableInteract;
        PauseController.OnPauseEnded   -= EnableInteract;
        base.OnDisable();
    }

    private void DisableInteract() => input.Player.Interact.Disable();
    private void EnableInteract()  => input.Player.Interact.Enable();

    private void OnEnterRange(Interactable interactable)
        => interactablesInRange.Add(interactable);

    private void OnExitRange(Interactable interactable)
    {
        interactablesInRange.Remove(interactable);
        interactable.SetPromptActive(false);
        if (currentInteractableObject == interactable)
            currentInteractableObject = null;
    }

    private void OnInteract(InputAction.CallbackContext ctx)
        => currentInteractableObject?.OnInteract();

    protected override void Update()
    {
        base.Update();
        UpdateCurrentInteractable();
    }

    private void UpdateCurrentInteractable()
    {
        interactablesInRange.RemoveAll(i => i == null);     // Borramos objetos destruidos

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
