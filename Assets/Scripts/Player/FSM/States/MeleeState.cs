using UnityEngine;

public class MeleeState : State
{
    float timer;
    const float cooldown = 1f;

    float meleeOffset = 1;
    Vector3 meleeBoxHalfExtents = new Vector3(0.5f, 0.8f, 0.8f);

    public MeleeState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado MELEE");
    }

    public override void Enter()
    {
        timer = cooldown;
        controller.RotateTowardCursor();

        Vector3 boxCenter = controller.transform.position + controller.transform.forward * meleeOffset + Vector3.up * 0.5f;
        Collider[] hits = Physics.OverlapBox(boxCenter, meleeBoxHalfExtents, controller.transform.rotation);
        DebugBoxDrawer.DrawBox(boxCenter, meleeBoxHalfExtents * 2f, controller.transform.rotation, new Color(1f, 0.4f, 0f, 0.6f), 0.5f);
        foreach (var hit in hits)
        {
            if (hit.transform == controller.transform) continue;
            // TO DO: Hacer daño
        }
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
    }
}