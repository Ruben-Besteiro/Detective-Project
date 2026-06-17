using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInvestigationController : PlayerController
{
    public static new PlayerInvestigationController Instance;

    List<Interactable> interactablesInRange = new();
    Interactable currentInteractableObject;
    public LinkableInteractable currentlySelectedLinkable;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Initialize();
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

    void DisableInteract() => input.Player.Interact.Disable();
    void EnableInteract()  => input.Player.Interact.Enable();

    void OnEnterRange(Interactable interactable)
        => interactablesInRange.Add(interactable);

    void OnExitRange(Interactable interactable)
    {
        interactablesInRange.Remove(interactable);
        interactable.SetPromptActive(false);
        if (currentInteractableObject == interactable)
            currentInteractableObject = null;
    }

    void OnInteract(InputAction.CallbackContext ctx)
        => currentInteractableObject?.OnInteract();

    void Update()
    {
        base.Update();
        UpdateCurrentInteractable();
    }

    void UpdateCurrentInteractable()
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
