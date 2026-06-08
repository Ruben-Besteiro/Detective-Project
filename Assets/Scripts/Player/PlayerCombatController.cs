using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    public static PlayerCombatController Instance;

    [SerializeField] PlayerController baseController;
    public InputActions input;

    public State currentState;
    public Vector2 MoveInput => baseController.input.Player.Move.ReadValue<Vector2>();

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
    }

    // TO DO: Añadir Attack, Dash, Sidestep, etc...
}
