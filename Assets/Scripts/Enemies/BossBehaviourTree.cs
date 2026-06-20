public class BossBehaviourTree : BehaviourTree<Enemy>
{
    protected override BehaviourNode<Enemy> GetRootNode()
    {
        return new SequenceNode<Enemy>(
            new PickAttackNode(),
            new SelectorNode<Enemy>(
                new Selection<Enemy>(ctx => ctx.agent.currentAttack == 0, new AttackMoveNode()),
                new Selection<Enemy>(ctx => ctx.agent.currentAttack == 1, new AttackProjectileNode()),
                new Selection<Enemy>(ctx => ctx.agent.currentAttack == 2, new AttackCircleNode())
            )
        );
    }
}