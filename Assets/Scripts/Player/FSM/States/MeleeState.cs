using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeleeState : State
{
    float timer;
    Vector3 meleeBoxHalfExtents;
    Vector3 aimPoint;
    bool hasAimPoint;

    public MeleeState(PlayerCombatController controller) : base(controller)
    {
        meleeBoxHalfExtents = new Vector3(
            controller.currentMeleeData.rangeX, 
            controller.currentMeleeData.rangeY, 
            controller.currentMeleeData.rangeZ
            );
    }

    public override void Enter()
    {
        timer = controller.currentMeleeData.cooldown;
        hasAimPoint = controller.HasAimPoint;
        if (hasAimPoint) aimPoint = controller.GetAimPoint();
        controller.RotateTowardCursor();

        controller.StartCoroutine(controller.IE_CheckMeleeHits());
    }

    public override void Update()
    {
        controller.Move();
        if (hasAimPoint)
            controller.RotateTowardCursor();

        timer -= Time.deltaTime;
        if (timer <= 0f)
            controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
    }
}