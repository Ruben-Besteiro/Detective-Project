using UnityEngine;

public class MoveState : State
{
    public MoveState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado MOVE");
    }

    public override void HandleInput()
    {
        if (controller.MoveInput.sqrMagnitude < 0.01f)
            controller.ChangeState(new IdleState(controller));
    }

    public override void Update()
    {
        controller.Move();
    }
}
