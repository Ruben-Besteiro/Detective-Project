using UnityEngine;

public class MeleeState : State
{
    float timer;
    const float cooldown = 1f;

    public MeleeState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado MELEE");
    }

    public override void Enter()
    {
        timer = cooldown;
        controller.RotateTowardCursor();
        controller.PerformMelee();
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            controller.ChangeState(controller.MoveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
    }
}