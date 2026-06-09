using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatController : MonoBehaviour
{
    public static PlayerCombatController Instance;

    [SerializeField] PlayerController baseController;
    public InputActions input;

    public State currentState;
    public Vector2 MoveInput => baseController.input.Player.Move.ReadValue<Vector2>();

    [Header("Cursor de Apuntado")]
    [SerializeField] LayerMask aimLayers;
    [SerializeField] GameObject cursorIndicatorPrefab;

    GameObject cursorIndicator;
    Vector3 cursorWorldPosition;
    bool cursorHasHit;

    [Header("Pistola")]
    [SerializeField] GameObject bulletPrefab;
    [SerializeField] float projectileSpeed = 10;
    [SerializeField] float projectileLifetime = 2;

    [Header("Cuchillo")]
    [SerializeField] float meleeOffset = 1;
    [SerializeField] Vector3 meleeBoxHalfExtents = new Vector3(0.5f, 0.8f, 0.8f);

    [Header("Dash")]
    [SerializeField] float dashSpeed = 25;

    [Header("Dodge")]
    [SerializeField] float dodgeSpeed = 25;

    public float DashSpeed  => dashSpeed;
    public float DodgeSpeed => dodgeSpeed;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        input = new InputActions();
        baseController.Initialize();
    }

    void Start()
    {
        baseController.speed *= 1.5f;
        currentState = new IdleState(this);
        currentState.Enter();
        cursorIndicator = Instantiate(cursorIndicatorPrefab);
        Cursor.visible = false;
    }

    public void ChangeState(State newState)
    {
        currentState.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Move() => baseController.Update();

    void OnEnable()
    {
        baseController.OnEnable();
        input.Player.Enable();
        input.Player.Shoot.started += OnShootAction;
        input.Player.Melee.started += OnMeleeAction;
        input.Player.Dash.started  += OnDashAction;
        input.Player.Dodge.started += OnDodgeAction;
    }

    void OnDisable()
    {
        input.Player.Shoot.started -= OnShootAction;
        input.Player.Melee.started -= OnMeleeAction;
        input.Player.Dash.started  -= OnDashAction;
        input.Player.Dodge.started -= OnDodgeAction;
        input.Player.Disable();
        baseController.OnDisable();
        cursorIndicator.SetActive(false);
    }

    void OnPause(InputAction.CallbackContext ctx)
    {
        baseController.OnPause(ctx);
    }

    void OnShootAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || (IsActionActive && currentState is not ShootState)) return;
        ChangeState(new ShootState(this));
    }

    void OnMeleeAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
        ChangeState(new MeleeState(this));
    }

    void OnDashAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
        Vector3 dir = GetMovementDirection();
        if (dir == Vector3.zero) dir = transform.forward;
        ChangeState(new DashState(this, dir));
    }

    void OnDodgeAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
        Vector3 dir = GetMovementDirection();
        if (dir == Vector3.zero) return;
        ChangeState(new DodgeState(this, dir));
    }

    Vector3 GetMovementDirection()
    {
        Vector2 raw = MoveInput;
        if (raw.sqrMagnitude < 0.01f) return Vector3.zero;
        return (MainCamera.IsoForward * raw.y + MainCamera.IsoRight * raw.x).normalized;
    }

    public void MoveInDirection(Vector3 dir, float speed) => baseController.MoveInDirection(dir, speed);

    void Update()
    {
        if (!PauseController.IsPaused)
        {
            currentState?.HandleInput();
            currentState?.Update();
        }
        UpdateCursorIndicator();
    }

    public void RotateTowardCursor()
    {
        if (!cursorHasHit) return;
        Vector3 dir = cursorWorldPosition - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;
    }

    public IEnumerator IE_Shoot()
    {
        float t = projectileLifetime;
        Vector3 spawnPos = transform.position + transform.forward + Vector3.up * 0.5f;
        GameObject bullet = Instantiate(bulletPrefab, spawnPos, transform.rotation);

        while (t > 0)
        {
            t -= Time.deltaTime;
            bullet.transform.position += bullet.transform.forward * projectileSpeed * Time.deltaTime;
            yield return null;
        }
        Destroy(bullet);
    }

    public void PerformMelee()
    {
        Vector3 boxCenter = transform.position + transform.forward * meleeOffset + Vector3.up * 0.5f;
        Collider[] hits = Physics.OverlapBox(boxCenter, meleeBoxHalfExtents, transform.rotation);
        DebugBoxDrawer.DrawBox(boxCenter, meleeBoxHalfExtents * 2f, transform.rotation, new Color(1f, 0.4f, 0f, 0.6f), 0.5f);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            Debug.Log($"Acuchillado: {hit.name}");
        }
    }

    void UpdateCursorIndicator()
    {
        if (cursorIndicator == null || Mouse.current == null) return;

        if (PauseController.IsPaused)
        {
            cursorIndicator.SetActive(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, aimLayers))
        {
            Debug.DrawRay(ray.origin, ray.direction * 200f, Color.red);
            Debug.Log($"Golpea: {hit.collider.name} en {hit.point}");
            cursorHasHit = true;
            cursorWorldPosition = hit.point;
            cursorIndicator.SetActive(true);
            cursorIndicator.transform.SetPositionAndRotation(
                hit.point + hit.normal * 0.02f,
                Quaternion.FromToRotation(Vector3.up, hit.normal));
        }
        else
        {
            cursorHasHit = false;
            cursorIndicator.SetActive(false);
        }
    }

    public IEnumerator IE_Intangible(float time)
    {
        // TO DO: Programar la intangibilidad de verdad
        GetComponent<Renderer>().material.color = Color.blue;
        yield return new WaitForSeconds(time);
        GetComponent<Renderer>().material.color = Color.gray;
    }

    bool IsActionActive =>
        currentState is ShootState ||
        currentState is MeleeState ||
        currentState is DashState  ||
        currentState is DodgeState;
}
