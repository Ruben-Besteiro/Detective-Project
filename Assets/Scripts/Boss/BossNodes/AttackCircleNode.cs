using System.Collections;
using UnityEngine;

public class AttackCircleNode : BehaviourNode<BossController>
{
    bool _running;

    public override State Start()
    {
        _running = true;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() => _running ? State.IN_PROGRESS : State.SUCCESS;

    IEnumerator Routine()
    {
        BossController boss = ctx.agent;

        for (int i = 0; i < boss.circleCount; i++)
        {
            float angle = i * Mathf.PI * 2f / boss.circleCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * boss.circleRadius;

            GameObject sphere = boss.SpawnProjectile(boss.transform.position + offset);
            sphere.GetComponent<Rigidbody>().AddForce(offset.normalized * boss.projectileForce, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(boss.attackCooldown);
        boss.currentAttack = -1;
        _running = false;
    }
}
