using UnityEngine;

public class DashState : State
{
    readonly Vector3 direction;
    float dashTimer;
    float cooldownTimer;

    public DashState(PlayerCombatController controller, Vector3 direction) : base(controller)
    {
        this.direction = direction;
    }

    public override void Enter()
    {
        dashTimer = controller.dashDuration;
        cooldownTimer = controller.dashCooldown;
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
