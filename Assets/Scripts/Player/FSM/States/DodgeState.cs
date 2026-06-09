using UnityEngine;

public class DodgeState : State
{
    readonly Vector3 direction;
    float dodgeTimer;
    float cooldownTimer;
    const float duration = 0.15f;
    const float cooldown = 0.5f;

    public DodgeState(PlayerCombatController controller, Vector3 direction) : base(controller)
    {
        this.direction = direction;
        Debug.Log("Has entrado en estado DODGE");
    }

    public override void Enter()
    {
        dodgeTimer = duration;
        cooldownTimer = cooldown;
        controller.StartCoroutine(controller.IE_Intangible(duration));
    }

    public override void Update()
    {
        if (dodgeTimer > 0f)
        {
            controller.MoveInDirection(direction, controller.DodgeSpeed);
            dodgeTimer -= Time.deltaTime;
        }
        else
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                controller.ChangeState(controller.MoveInput.sqrMagnitude > 0.01f
                    ? (State)new MoveState(controller)
                    : new IdleState(controller));
        }
    }
}
