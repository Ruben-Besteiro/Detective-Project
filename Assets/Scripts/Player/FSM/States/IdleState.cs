using UnityEngine;
using UnityEngine.InputSystem;

public class IdleState : State
{
    public IdleState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado IDLE");
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

        if (controller.MoveInput.sqrMagnitude > 0.01f)
            controller.ChangeState(new MoveState(controller));
    }
}