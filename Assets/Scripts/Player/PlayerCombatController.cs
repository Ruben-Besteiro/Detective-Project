using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatController : PlayerController
{
    public static PlayerCombatController Instance;

    public State currentState;
    public Vector2 moveInput;

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

    [Header("Lock-On")]
    [SerializeField] GameObject boss;
    [SerializeField] float minRadius = 1.5f;
    [SerializeField] float maxRadius = 15f;
    [SerializeField] GameObject reticlePrefab;

    public bool isLockedOn;
    public Transform lockOnTarget;

    float lockOnAngle;
    float lockOnRadius;
    GameObject lockOnIndicator;

    public float DashSpeed => dashSpeed;
    public float DodgeSpeed => dodgeSpeed;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        Initialize();
    }

    void Start()
    {
        speed *= 1.5f;
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

    public void Move()
    {
        if (isLockedOn)
            UpdateLockOnMovement(moveInput);
        else
            base.Update();
    }

    void OnEnable()
    {
        base.OnEnable();
        input.Player.Enable();
        input.Player.Shoot.started += OnShootAction;
        input.Player.Melee.started += OnMeleeAction;
        input.Player.Dash.started += OnDashAction;
        input.Player.Dodge.started += OnDodgeAction;
        input.Player.LockOn.started += OnLockOnAction;
        input.Player.LockOn.canceled += OnLockOnCanceled;
    }

    void OnDisable()
    {
        input.Player.Shoot.started -= OnShootAction;
        input.Player.Melee.started -= OnMeleeAction;
        input.Player.Dash.started -= OnDashAction;
        input.Player.Dodge.started -= OnDodgeAction;
        input.Player.LockOn.started -= OnLockOnAction;
        input.Player.LockOn.canceled -= OnLockOnCanceled;
        input.Player.Disable();
        base.OnDisable();
        cursorIndicator.SetActive(false);
    }

    void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();
        if (!PauseController.IsPaused)
        {
            currentState?.HandleInput();
            currentState?.Update();
        }
        UpdateCursorIndicator();
    }

    // --- ACCIONES BÁSICAS ---

    void OnPause(InputAction.CallbackContext ctx)
    {
        base.OnPause(ctx);
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

    // --- LOCK-ON ---

    void OnLockOnAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused) return;
        if (boss != null) ActivateLockOn(boss.transform);
    }

    void OnLockOnCanceled(InputAction.CallbackContext ctx)
    {
        DeactivateLockOn();
    }

    void ActivateLockOn(Transform target)
    {
        lockOnTarget = target;
        isLockedOn = true;

        Vector3 toPlayer = transform.position - target.position;
        toPlayer.y = 0f;
        lockOnRadius = Mathf.Clamp(toPlayer.magnitude, minRadius, maxRadius);
        lockOnAngle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;

        if (reticlePrefab != null && lockOnIndicator == null)
            lockOnIndicator = Instantiate(reticlePrefab, target);
        lockOnIndicator?.SetActive(true);
    }

    public void DeactivateLockOn()
    {
        isLockedOn = false;
        lockOnTarget = null;
        lockOnIndicator?.SetActive(false);
    }

    void UpdateLockOnMovement(Vector2 moveInput)
    {
        if (!isLockedOn || lockOnTarget == null) return;

        float speed = this.speed;
        lockOnAngle += moveInput.x * (speed / lockOnRadius) * Mathf.Rad2Deg * Time.deltaTime;
        lockOnRadius -= moveInput.y * speed * Time.deltaTime;
        lockOnRadius = Mathf.Clamp(lockOnRadius, minRadius, maxRadius);

        float rad = lockOnAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = lockOnTarget.position + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * lockOnRadius;

        Vector3 toOrbit = orbitPos - transform.position;
        toOrbit.y = 0f;

        cc.SimpleMove(moveInput.sqrMagnitude > 0.01f ? toOrbit / Time.deltaTime : Vector3.zero);

        // Rotar para mirar al enemigo
        if (lockOnTarget == null) return;
        Vector3 look = lockOnTarget.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.forward = look.normalized;
    }

    // --- UTILIDADES ---

    // Esto se llama desde ShootState y MeleeState respectivamente

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
            // TO DO: Hacer daño
        }
    }

    void UpdateCursorIndicator()
    {
        if (cursorIndicator == null || Mouse.current == null) return;

        if (PauseController.IsPaused || isLockedOn)
        {
            cursorIndicator.SetActive(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, aimLayers))
        {
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

    Vector3 GetMovementDirection()
    {
        Vector2 raw = moveInput;
        if (raw.sqrMagnitude < 0.01f) return Vector3.zero;
        return (MainCamera.isoForward * raw.y + MainCamera.isoRight * raw.x).normalized;
    }

    public void MoveInDirection(Vector3 dir, float spd) => cc.SimpleMove(dir * spd);

    public void RotateTowardCursor()
    {
        if (isLockedOn || !cursorHasHit) return;
        Vector3 dir = cursorWorldPosition - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;
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
