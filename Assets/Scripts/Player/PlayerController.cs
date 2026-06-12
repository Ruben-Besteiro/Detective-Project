using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public abstract class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] public float speed = 5f;

    public CharacterController cc;
    public InputActions input;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Initialize();
    }

    public void Initialize()
    {
        if (input != null) return;
        input = new InputActions();
        cc = GetComponent<CharacterController>();
    }


    protected virtual void OnEnable()
    {
        input.Player.Enable();
        input.Player.Pause.performed += OnPause;
        DialogueManager.OnDialogueStarted  += DisableMovement;
        DialogueManager.OnDialogueFinished += EnableMovement;
        PauseController.OnPauseStarted += DisableMovement;
        PauseController.OnPauseEnded  += EnableMovement;
    }

    protected virtual void OnDisable()
    {
        input.Player.Pause.performed -= OnPause;
        DialogueManager.OnDialogueStarted  -= DisableMovement;
        DialogueManager.OnDialogueFinished -= EnableMovement;
        PauseController.OnPauseStarted -= DisableMovement;
        PauseController.OnPauseEnded  -= EnableMovement;
        input.Player.Disable();
    }

    protected void DisableMovement() => input.Player.Move.Disable();
    protected void EnableMovement() => input.Player.Move.Enable();

    protected virtual void OnPause(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused) PauseController.Unpause();
        else PauseController.Pause();
    }

    protected void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude < 0.01f) return;

        // Movimiento isométrico
        Vector3 dir = (MainCamera.isoForward * raw.y + MainCamera.isoRight * raw.x).normalized;
        cc.SimpleMove(dir * speed);
        transform.forward = dir;
    }

    public void MoveInDirection(Vector3 worldDir, float speed)
    {
        cc.SimpleMove(worldDir * speed);
    }
}