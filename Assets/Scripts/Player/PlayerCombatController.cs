using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;

public class PlayerCombatController : PlayerController
{
    public static PlayerCombatController Instance;
    private float combatSpeed;
    protected override float MoveSpeed => combatSpeed;
    private float currentHp;

    public State currentState;
    public Vector2 moveInput;

    [Header("Cursor de Apuntado")]
    [SerializeField] LayerMask aimLayers;
    [SerializeField] GameObject reticlePrefab;
    GameObject reticle;
    Vector3 cursorWorldPosition;
    bool cursorHasHit;
    Transform enemyThatTheReticleIsOnTopOf;

    [Header("Armas")]
    public GunData currentGunData;
    public MeleeData currentMeleeData;

    [Header("Dash")]
    [SerializeField] public float dashSpeed = 25;

    [Header("Dodge")]
    [SerializeField] public float dodgeSpeed = 25;

    [Header("Lock-On")]
    [SerializeField] GameObject boss;       // Solo al jefe
    [SerializeField] float maxDistance = 15;
    [SerializeField] float minDistance = 2;     // Debe ser suficiente como para pegar con el cuchillo

    public bool isLockedOn;
    public Transform lockOnTarget;

    float lockOnAngle;
    float lockOnRadius;

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
        combatSpeed = stats.speed * 1.5f;
        currentHp = stats.maxHp;
        currentState = new IdleState(this);
        currentState.Enter();
        reticle = Instantiate(reticlePrefab);
        Cursor.visible = false;
        if (boss != null) minDistance += boss.GetComponent<Collider>().bounds.size.z / 2;
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

    protected override void OnEnable()
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

    protected override void OnDisable()
    {
        input.Player.Shoot.started -= OnShootAction;
        input.Player.Melee.started -= OnMeleeAction;
        input.Player.Dash.started -= OnDashAction;
        input.Player.Dodge.started -= OnDodgeAction;
        input.Player.LockOn.started -= OnLockOnAction;
        input.Player.LockOn.canceled -= OnLockOnCanceled;
        input.Player.Disable();
        base.OnDisable();
        reticle.SetActive(false);
    }

    private void Update()
    {
        moveInput = input.Player.Move.ReadValue<Vector2>();
        if (!PauseController.IsPaused)
        {
            currentState?.HandleInput();
            currentState?.Update();
        }
        UpdateReticle();
    }

    // --- ACCIONES BÁSICAS ---

    protected override void OnPause(InputAction.CallbackContext ctx)
    {
        base.OnPause(ctx);
    }

    private void OnShootAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || (IsActionActive && currentState is not ShootState)) return;
        ChangeState(new ShootState(this));
    }

    private void OnMeleeAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
        ChangeState(new MeleeState(this));
    }

    private void OnDashAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
        Vector3 dir = GetMovementDirection();
        if (dir == Vector3.zero) dir = transform.forward;
        ChangeState(new DashState(this, dir));
    }

    private void OnDodgeAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
        Vector3 dir = GetMovementDirection();
        if (dir == Vector3.zero) return;
        ChangeState(new DodgeState(this, dir));
    }

    // --- LOCK-ON ---

    private void OnLockOnAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused) return;
        if (boss != null && Vector3.Distance(transform.position, boss.transform.position) < maxDistance && Vector3.Distance(transform.position, boss.transform.position) > minDistance)
            ActivateLockOn(boss.transform);
    }

    private void OnLockOnCanceled(InputAction.CallbackContext ctx)
    {
        DeactivateLockOn();
    }

    private void ActivateLockOn(Transform target)
    {
        if (Vector3.Distance(transform.position, target.position) > maxDistance)
        {
            isLockedOn = false;
            return;
        }
        isLockedOn = true;
        lockOnTarget = target;

        Vector3 toPlayer = transform.position - target.position;
        toPlayer.y = 0f;
        lockOnRadius = Mathf.Clamp(toPlayer.magnitude, minDistance, maxDistance);
        lockOnAngle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;
    }

    private void DeactivateLockOn()
    {
        isLockedOn = false;
        lockOnTarget = null;
    }

    private void UpdateLockOnMovement(Vector2 moveInput)
    {
        if (!isLockedOn || lockOnTarget == null) return;

        float sp = this.combatSpeed;
        lockOnAngle += moveInput.x * (sp / lockOnRadius) * Mathf.Rad2Deg * Time.deltaTime;
        lockOnRadius -= moveInput.y * sp * Time.deltaTime;
        lockOnRadius = Mathf.Clamp(lockOnRadius, minDistance, maxDistance);

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

    private void UpdateReticle()
    {
        if (reticle == null || Mouse.current == null) return;

        if (PauseController.IsPaused || isLockedOn)
        {
            reticle.SetActive(false);
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out RaycastHit hit, 200f, aimLayers))
        {
            cursorHasHit = true;
            cursorWorldPosition = hit.point;
            enemyThatTheReticleIsOnTopOf = hit.collider.CompareTag("Enemy") ? hit.collider.transform : null;
            reticle.SetActive(true);
            reticle.transform.SetPositionAndRotation(
                hit.point + hit.normal * 0.02f,
                Quaternion.FromToRotation(Vector3.up, hit.normal));
        }
        else
        {
            cursorHasHit = false;
            enemyThatTheReticleIsOnTopOf = null;
            reticle.SetActive(false);
        }
    }

    public Vector3 GetMovementDirection()
    {
        Vector2 raw = moveInput;
        if (raw.sqrMagnitude < 0.01f) return Vector3.zero;
        return (MainCamera.isoForward * raw.y + MainCamera.isoRight * raw.x).normalized;
    }

    public void RotateTowardCursor()
    {
        if (isLockedOn || !cursorHasHit) return;
        Vector3 target = enemyThatTheReticleIsOnTopOf != null ? enemyThatTheReticleIsOnTopOf.position : cursorWorldPosition;
        Vector3 dir = target - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;
    }

    public IEnumerator IE_CheckMeleeHits()
    {
        HashSet<Enemy> damagedEnemies = new();

        Vector3 boxCenter = transform.position + transform.forward * (currentMeleeData.hitboxOffset + currentMeleeData.rangeZ / 2) + Vector3.up * 0.5f;
        Vector3 boxHalfExtents = new Vector3(currentMeleeData.rangeXY, currentMeleeData.rangeXY, currentMeleeData.rangeZ);
        DebugBoxDrawer.DrawBox(boxCenter, boxHalfExtents * 2f, transform.rotation, new Color(1f, 0.4f, 0f, 0.6f), 0.5f);

        for (int i = 0; i < currentMeleeData.activeFrames; i++)
        {
            Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, transform.rotation);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                if (hit.TryGetComponent<Enemy>(out var enemy))
                {
                    if (damagedEnemies.Contains(enemy)) continue;
                    enemy.TakeDamage(currentMeleeData.damage);
                    damagedEnemies.Add(enemy);
                }
            }

            yield return null;
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