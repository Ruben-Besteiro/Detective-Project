using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : State
{
    public IdleState(PlayerCombatController controller) : base(controller) {}

    public override void Update()
    {
        if (controller.isLockedOn)
            controller.Move();
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

        if (controller.moveInput.sqrMagnitude > 0.01f)
            controller.ChangeState(new MoveState(controller));
    }
}