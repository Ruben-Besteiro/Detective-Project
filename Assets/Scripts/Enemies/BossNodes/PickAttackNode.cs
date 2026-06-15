using UnityEngine;

public class PickAttackNode : BehaviourNode<BossController>
{
    public override State Start()
    {
        //ctx.agent.currentAttack = Random.Range(0, 3);
        ctx.agent.currentAttack = 0;
        Debug.Log("El jefe ha escogido el ataque número: " + ctx.agent.currentAttack);
        return State.SUCCESS;
    }

    public override State Update()
        => State.SUCCESS;
}
