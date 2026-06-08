using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class PlayerCombatController : MonoBehaviour
{
    public static PlayerCombatController Instance;

    [SerializeField] PlayerController baseController;
    public InputActions input;

    public State currentState;
    public Vector2 MoveInput => baseController.input.Player.Move.ReadValue<Vector2>();

    [Header("Cursor de Apuntado")]
    [Tooltip("Capas sobre las que se hace el raycast. Excluye la capa del jugador para evitar que se detecte a sí mismo.")]
    [SerializeField] LayerMask aimLayers = ~0;
    [Tooltip("Prefab opcional. Si no se asigna se crea un disco por defecto.")]
    [SerializeField] GameObject cursorIndicatorPrefab;
    [SerializeField] Camera mainCamera;

    GameObject cursorIndicator;

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
    }

    void OnDisable()
    {
        baseController.OnDisable();
        if (cursorIndicator != null)
            cursorIndicator.SetActive(false);
    }

    void OnPause(InputAction.CallbackContext ctx)
    {
        baseController.OnPause(ctx);
    }

    void Update()
    {
        if (!PauseController.IsPaused)
        {
            currentState?.HandleInput();
            currentState?.Update();
        }
        UpdateCursorIndicator();
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
            cursorIndicator.SetActive(true);
            cursorIndicator.transform.SetPositionAndRotation(
                hit.point + hit.normal * 0.02f,
                Quaternion.FromToRotation(Vector3.up, hit.normal));
        }
        else
        {
            cursorIndicator.SetActive(false);
        }
    }

    // TO DO: Añadir Attack, Dash, Sidestep, etc...
}
