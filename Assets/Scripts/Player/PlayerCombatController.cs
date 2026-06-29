using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class PlayerCombatController : PlayerController
{
    public static PlayerCombatController Instance;
    private float combatSpeed;
    protected override float MoveSpeed => combatSpeed;      // Esto nos permite tener varias velocidades según la escena
    public float currentHp;
    public bool isIntangible = false;

    public State currentState;      // Para la máquina de estados
    public Vector2 moveInput;
    public TextMeshProUGUI playerNameText;
    [SerializeField] Image lifeBar;

    [Header("Cursor de Apuntado")]
    [SerializeField] GameObject reticlePrefab;
    GameObject reticle;
    Vector3 cursorWorldPosition;
    bool cursorHasHit;
    Transform enemyThatTheReticleIsOnTopOf;

    [Header("Armas")]
    public GunData currentGunData;
    public MeleeData currentMeleeData;
    public GameObject bulletPrefab;
    public float damageMultiplier = 1;      // En el Start se cambia a 1.5 si elegiste la hipótesis correcta

    [Header("Lock-On")]
    [SerializeField] private GameObject boss;       // Solo al jefe
    [SerializeField] private float maxDistance = 15;
    [SerializeField] private float minDistance = 2;     // Debe ser suficiente como para pegar con el cuchillo

    public bool isLockedOn;
    public Transform lockOnTarget;

    float lockOnAngle;
    float lockOnRadius;

    protected override void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        base.Awake();
    }

    private void Start()
    {
        combatSpeed = stats.speed * 1.5f;
        currentHp = stats.maxHp;
        damageMultiplier = GameDataManager.Instance.currentHypothesis == Hypotheses.H1 ? 1.5f : 1;
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

    // Esto se llama desde el MoveState, DashState y DodgeState
    public void Move()
    {
        if (isLockedOn)
            UpdateLockOnMovement(moveInput);
        else
            base.Update();      // Movimiento básico
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

    protected override void Update()
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

    private void OnShootAction(InputAction.CallbackContext ctx)
    {
        if (PauseController.IsPaused || IsActionActive) return;
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
        Vector3 dir = isLockedOn && lockOnTarget != null ? GetLockOnDodgeDirection() : GetMovementDirection();
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

    // En lock-on, el jugador se mueve alrededor del enemigo
    private void UpdateLockOnMovement(Vector2 moveInput)
    {
        if (!isLockedOn || lockOnTarget == null) return;

        // Recalculamos ángulo y radio desde la posición actual del jugador cada frame por si el enemigo se mueve
        Vector3 toPlayer = transform.position - lockOnTarget.position;
        toPlayer.y = 0f;
        float actualDistance = toPlayer.magnitude;

        if (actualDistance > maxDistance)
        {
            DeactivateLockOn();
            return;
        }

        lockOnRadius = actualDistance;
        lockOnAngle = Mathf.Atan2(toPlayer.x, toPlayer.z) * Mathf.Rad2Deg;

        float sp = this.combatSpeed;
        lockOnAngle -= moveInput.x * (sp / Mathf.Max(lockOnRadius, 0.1f)) * Mathf.Rad2Deg * Time.deltaTime;
        lockOnRadius -= moveInput.y * sp * Time.deltaTime;
        if (lockOnRadius > maxDistance)
        {
            DeactivateLockOn();
            return;
        }
        lockOnRadius = Mathf.Max(lockOnRadius, minDistance);

        float rad = lockOnAngle * Mathf.Deg2Rad;
        Vector3 orbitPos = lockOnTarget.position + new Vector3(Mathf.Sin(rad), 0f, Mathf.Cos(rad)) * lockOnRadius;

        Vector3 toOrbit = orbitPos - transform.position;
        toOrbit.y = 0f;

        cc.SimpleMove(moveInput.sqrMagnitude > 0.01f ? toOrbit / Time.deltaTime : Vector3.zero);

        Vector3 look = lockOnTarget.position - transform.position;
        look.y = 0f;
        if (look.sqrMagnitude > 0.001f)
            transform.forward = look.normalized;
    }

    private Vector3 GetLockOnDodgeDirection()
    {
        if (moveInput.sqrMagnitude < 0.01f) return Vector3.zero;
        Vector3 toEnemy = lockOnTarget.position - transform.position;
        toEnemy.y = 0f;
        toEnemy.Normalize();
        Vector3 right = Vector3.Cross(Vector3.up, toEnemy);
        return (toEnemy * moveInput.y + right * moveInput.x).normalized;
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
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
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

    
    // Esto se utiliza cuando atacamos sin lock on => miramos hacia la retícula aunque nos movamos

    public bool HasAimPoint => !isLockedOn && cursorHasHit;

    // El punto al que atacaremos
    public Vector3 GetAimPoint()
    {
        return enemyThatTheReticleIsOnTopOf != null ? enemyThatTheReticleIsOnTopOf.position : reticle.transform.position;
    }

    public void RotateTowardCursor()
    {
        if (!HasAimPoint) return;
        Vector3 dir = GetAimPoint() - transform.position;
        dir.y = 0;
        if (dir.sqrMagnitude > 0.001f)
            transform.forward = dir.normalized;
    }

    public IEnumerator IE_CheckMeleeHits()
    {
        HashSet<Enemy> damagedEnemies = new();
        Vector3 boxHalfExtents = new Vector3(currentMeleeData.rangeX, currentMeleeData.rangeY, currentMeleeData.rangeZ);

        int activeFrames = Mathf.RoundToInt(currentMeleeData.activeDuration / Time.deltaTime);
        for (int i = 0; i < activeFrames; i++)
        {
            // Recalculado cada frame porque el jugador puede moverse durante el ataque
            Vector3 boxCenter = transform.position + transform.forward * (currentMeleeData.hitboxOffset + currentMeleeData.rangeZ / 2) + Vector3.up * 0.5f;
            DebugBoxDrawer.DrawBox(boxCenter, boxHalfExtents * 2f, transform.rotation, new Color(1f, 0.4f, 0f, 0.6f), 0.5f);

            Collider[] hits = Physics.OverlapBox(boxCenter, boxHalfExtents, transform.rotation);
            foreach (var hit in hits)
            {
                if (hit.transform == transform) continue;
                if (hit.TryGetComponent<Enemy>(out var enemy))
                {
                    if (damagedEnemies.Contains(enemy)) continue;
                    enemy.TakeDamage(currentMeleeData.damage * damageMultiplier);
                    damagedEnemies.Add(enemy);
                }
            }

            yield return null;
        }
    }

    // --- DAÑO ---

    // Daño por contacto de los enemigos
    public void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject.name.StartsWith("Arm")) return;    // Se hace en el ArmHitbox
            TakeDamage(1, (transform.position - other.transform.position).normalized, 0.5f, 0.5f);
        }
    }

    public void TakeDamage(float dmg, Vector3 dir, float knockbackDuration, float intangibilityDuration, float knockbackSpeed = 25f)
    {
        if (isIntangible) return;

        StartCoroutine(IE_Knockback(dir, knockbackDuration, knockbackSpeed));
        StartCoroutine(IE_Intangible(intangibilityDuration));

        currentHp -= dmg;
        if (currentHp > 0)
        {
            lifeBar.fillAmount = currentHp / stats.maxHp;
            ChangeState(new HurtState(this, knockbackDuration));        // Desactivar input
        }
        else
            Destroy(gameObject);
    }

    private IEnumerator IE_Knockback(Vector3 dir, float knockbackDuration, float knockbackSpeed)
    {
        if (isIntangible) yield break;

        float elapsed = 0f;

        while (elapsed < knockbackDuration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            float t = knockbackDuration - elapsed;
            cc.Move(dir * Time.deltaTime * knockbackSpeed * t);
        }
    }

    public IEnumerator IE_Intangible(float intangibilityDuration)
    {
        isIntangible = true;
        var renderers = GetComponentsInChildren<Renderer>();
        foreach (var r in renderers) r.material.color = Color.blue;
        yield return new WaitForSeconds(intangibilityDuration);
        foreach (var r in renderers) r.material.color = Color.gray;
        isIntangible = false;
    }

    bool IsActionActive =>
        currentState is ShootState ||
        currentState is MeleeState ||
        currentState is DashState  ||
        currentState is DodgeState ||
        currentState is HurtState;
}