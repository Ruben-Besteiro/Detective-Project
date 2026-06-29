using UnityEngine;
using UnityEngine.InputSystem;

public class MoveState : State
{
    public MoveState(PlayerCombatController controller) : base(controller) {}

    public override void HandleInput()
    {
        if (Mouse.current != null)
        {
            if (Mouse.current.leftButton.isPressed)
            {
                controller.ChangeState(new ShootState(controller));
                return;
            }
            if (Mouse.current.rightButton.isPressed)
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