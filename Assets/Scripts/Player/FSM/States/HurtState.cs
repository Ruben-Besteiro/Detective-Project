using UnityEngine;

public class HurtState : State
{
    float timer;
    float duration;

    public HurtState(PlayerCombatController controller, float duration) : base(controller)
    {
        this.duration = duration;
    }

    public override void Enter()
    {
        timer = duration;
    }

    // Aquí lo único que realmente ocurre es desactivar el input
    public override void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
        }
        else
        {
            controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
        }
    }
}
