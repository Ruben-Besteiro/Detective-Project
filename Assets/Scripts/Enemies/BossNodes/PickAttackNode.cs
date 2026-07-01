using UnityEngine;

public class PickAttackNode : BehaviourNode<Enemy>
{
    public override State Start()
    {
        if (ctx.agent.TryGetComponent<Boss1Controller>(out _))
            ctx.agent.currentAttack = Random.Range(0, 3);
        else            // Minions
            ctx.agent.currentAttack = 0;
        return State.SUCCESS;
    }

    public override State Update()
        => State.SUCCESS;
}