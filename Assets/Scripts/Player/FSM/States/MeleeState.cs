using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class MeleeState : State
{
    float timer;
    Vector3 meleeBoxHalfExtents = new Vector3(PlayerCombatController.Instance.currentMeleeData.rangeXY, PlayerCombatController.Instance.currentMeleeData.rangeXY, PlayerCombatController.Instance.currentMeleeData.rangeZ);
    float meleeOffset = 1;

    public MeleeState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado MELEE");
    }

    public override void Enter()
    {
        timer = controller.currentMeleeData.endLag;
        controller.RotateTowardCursor();

        controller.StartCoroutine(controller.IE_CheckMeleeHits());
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