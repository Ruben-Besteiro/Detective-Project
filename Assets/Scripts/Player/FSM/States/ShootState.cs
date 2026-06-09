using UnityEngine;

public class ShootState : State
{
    float timer;
    const float cooldown = 1f;

    public ShootState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado SHOOT");
    }

    public override void Enter()
    {
        timer = cooldown;
        controller.RotateTowardCursor();
        controller.StartCoroutine(controller.IE_Shoot());
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