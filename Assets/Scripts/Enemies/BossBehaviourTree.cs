public class BossBehaviourTree : BehaviourTree<Enemy>
{
    protected override BehaviourNode<Enemy> GetRootNode()
    {
        return new SequenceNode<Enemy>(
            new PickAttackNode(),
            new SelectorNode<Enemy>(
                new Selection<Enemy>(ctx => ctx.agent.currentAttack == 0, new ArmAttackNode()),
                new Selection<Enemy>(ctx => ctx.agent.currentAttack == 1, new RangedAttackNode()),
                new Selection<Enemy>(ctx => ctx.agent.currentAttack == 2, new SpawnMinionsNode())
            )
        );
    }
}