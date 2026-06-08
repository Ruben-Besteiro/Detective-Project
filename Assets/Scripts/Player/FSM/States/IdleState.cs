using UnityEngine;

public class IdleState : State
{
    public IdleState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado IDLE");
    }

    public override void HandleInput()
    {
        if (controller.MoveInput.sqrMagnitude > 0.01f)
            controller.ChangeState(new MoveState(controller));
    }
}
