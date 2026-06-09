using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] public float speed = 5f;

    public CharacterController cc;
    public InputActions input;

    void Awake()
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


    public void OnEnable()
    {
        input.Player.Enable();
        input.Player.Pause.performed += OnPause;
        DialogueManager.OnDialogueStarted  += DisableMovement;
        DialogueManager.OnDialogueFinished += EnableMovement;
        PauseController.OnPauseStarted += DisableMovement;
        PauseController.OnPauseEnded  += EnableMovement;
    }

    public void OnDisable()
    {
        input.Player.Pause.performed -= OnPause;
        DialogueManager.OnDialogueStarted  -= DisableMovement;
        DialogueManager.OnDialogueFinished -= EnableMovement;
        PauseController.OnPauseStarted -= DisableMovement;
        PauseController.OnPauseEnded  -= EnableMovement;
        input.Player.Disable();
    }

    void DisableMovement() => input.Player.Move.Disable();
    void EnableMovement() => input.Player.Move.Enable();

    public void OnPause(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused) PauseController.Unpause();
        else PauseController.Pause();
    }

    public void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude < 0.01f) return;

        Vector3 dir = (MainCamera.IsoForward * raw.y + MainCamera.IsoRight * raw.x).normalized;
        cc.SimpleMove(dir * speed);
        transform.forward = dir;
    }
}