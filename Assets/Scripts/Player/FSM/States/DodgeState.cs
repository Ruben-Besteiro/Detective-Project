using UnityEngine;

public class DodgeState : State
{
    readonly Vector3 direction;
    float dodgeTimer;
    float cooldownTimer;

    public DodgeState(PlayerCombatController controller, Vector3 direction) : base(controller)
    {
        this.direction = direction;
        Debug.Log("Has entrado en estado DODGE");
    }

    public override void Enter()
    {
        dodgeTimer = controller.dodgeDuration;
        cooldownTimer = controller.dodgeCooldown;
        controller.StartCoroutine(controller.IE_Intangible(dodgeTimer));
    }

    public override void Update()
    {
        if (dodgeTimer > 0f)
        {
            controller.MoveInDirection(direction, controller.dodgeSpeed);
            dodgeTimer -= Time.deltaTime;
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
