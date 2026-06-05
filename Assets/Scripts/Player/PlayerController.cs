using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] public float speed = 5f;

    CharacterController cc;
    public InputActions input;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

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
        input.Player.Disable();
        input.Player.Pause.performed -= OnPause;
        DialogueManager.OnDialogueStarted  -= DisableMovement;
        DialogueManager.OnDialogueFinished -= EnableMovement;
        PauseController.OnPauseStarted -= DisableMovement;
        PauseController.OnPauseEnded  -= EnableMovement;
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