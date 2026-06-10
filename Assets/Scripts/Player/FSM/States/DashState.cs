using UnityEngine;

public class DashState : State
{
    readonly Vector3 direction;
    float dashTimer;
    float cooldownTimer;
    const float duration = 0.33f;
    const float cooldown = 0.5f;

    public DashState(PlayerCombatController controller, Vector3 direction) : base(controller)
    {
        this.direction = direction;
        Debug.Log("Has entrado en estado DASH");
    }

    public override void Enter()
    {
        dashTimer = duration;
        cooldownTimer = cooldown;
    }

    public override void Update()
    {
        if (dashTimer > 0f)
        {
            controller.MoveInDirection(direction, controller.dashSpeed);
            dashTimer -= Time.deltaTime;
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                    ? (State)new MoveState(controller)
                    : new IdleState(controller));
        }
    }
}
