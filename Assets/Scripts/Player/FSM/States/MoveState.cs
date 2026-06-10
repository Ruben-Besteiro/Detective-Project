using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : State
{
    public MoveState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado MOVE");
    }

    public override void HandleInput()
    {
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                controller.ChangeState(new ShootState(controller));
                return;
            }
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                controller.ChangeState(new MeleeState(controller));
                return;
            }
        }

        if (controller.moveInput.sqrMagnitude < 0.01f)
            controller.ChangeState(new IdleState(controller));
    }

    public override void Update()
    {
        controller.Move();
    }
}