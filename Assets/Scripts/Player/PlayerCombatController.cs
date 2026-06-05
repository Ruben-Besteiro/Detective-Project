using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{
    public static PlayerCombatController Instance;

    // Patrón Decorator
    [SerializeField] PlayerController baseController;       // DEBE ESTAR DESACTIVADO EN EL INSPECTOR
    public InputActions input;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        input = new InputActions();
    }

    void Start()
    {
        baseController.speed *= 1.5f;
    }

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

    public void Update()
    {
        baseController.Update();
    }

    // TO DO: Añadir Attack, Dash, Sidestep, etc...
}
