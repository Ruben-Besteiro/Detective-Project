using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed = 5f;

    InputActions input;
    CharacterController cc;

    void Awake()
    {
        input = new InputActions();
        cc = GetComponent<CharacterController>();
    }

    void OnEnable() => input.Player.Enable();
    void OnDisable() => input.Player.Disable();

    void Update()
    {
        Vector2 raw = input.Player.Move.ReadValue<Vector2>();
        if (raw.sqrMagnitude < 0.01f) return;

        // Movimiento relativo a la cámara proyectado en el plano XZ
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight   = cam.right;   camRight.y   = 0f; camRight.Normalize();

        Vector3 dir = (camForward * raw.y + camRight * raw.x).normalized;
        cc.SimpleMove(dir * speed);
        transform.forward = dir;
    }
}
