using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    CharacterController cc;
    InputActions input;
    InteractableObject currentInteractable;

    void Awake()
    {
        input = new InputActions();
        cc = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        input.Player.Enable();
        InteractableObject.OnFocusChanged += OnFocusChanged;
        input.Player.Interact.performed += OnInteract;
        input.Player.Pause.performed += OnPause;
        DialogueManager.OnDialogueStarted  += OnDialogueStarted;
        DialogueManager.OnDialogueFinished += OnDialogueFinished;
        PauseController.OnPauseStarted += DisableInput;
        PauseController.OnPauseEnded += EnableInput;
    }
    void OnDisable()
    {
        input.Player.Disable();
        InteractableObject.OnFocusChanged -= OnFocusChanged;
        input.Player.Interact.performed -= OnInteract;
        input.Player.Pause.performed -= OnPause;
        DialogueManager.OnDialogueStarted  -= OnDialogueStarted;
        DialogueManager.OnDialogueFinished -= OnDialogueFinished;
        PauseController.OnPauseStarted -= DisableInput;
        PauseController.OnPauseEnded -= EnableInput;
    }

    //Activamos y desactivamos controles al pausar el juego
    private void DisableInput()
    {
        input.Player.Move.Disable();
        input.Player.Interact.Disable();
    }

    private void EnableInput()
    {
        input.Player.Move.Enable();
        input.Player.Interact.Enable();
    }

    void OnPause(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused) PauseController.Unpause();
        else
        { PauseController.Pause(); }
    }

    void OnDialogueStarted()  => input.Player.Disable();
    void OnDialogueFinished() => input.Player.Enable();

    void OnFocusChanged(InteractableObject obj) => currentInteractable = obj;

    void OnInteract(InputAction.CallbackContext ctx) => currentInteractable?.Interact();

    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude < 0.01f) return;

        // Movimiento relativo a la cámara proyectado en el plano XZ
        Transform cam = Camera.main.transform;
        Vector3 camF = cam.forward;
        Vector3 camR = cam.right;
        camF.y = 0f;
        camF.Normalize();
        camR.y = 0f;
        camR.Normalize();

        Vector3 dir = (camF * raw.y + camR * raw.x).normalized;
        cc.SimpleMove(dir * speed);
        transform.forward = dir;
    }
}
