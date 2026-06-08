public class BossBehaviourTree : BehaviourTree<BossController>
{
    protected override BehaviourNode<BossController> GetRootNode()
    {
        return new SequenceNode<BossController>(
            new PickAttackNode(),
            new SelectorNode<BossController>(
                new Selection<BossController>(ctx => ctx.agent.currentAttack == 0, new AttackMoveNode()),
                new Selection<BossController>(ctx => ctx.agent.currentAttack == 1, new AttackProjectileNode()),
                new Selection<BossController>(ctx => ctx.agent.currentAttack == 2, new AttackCircleNode())
            )
        );
    }
}
