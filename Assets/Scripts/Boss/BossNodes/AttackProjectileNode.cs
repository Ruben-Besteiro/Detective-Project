using System.Collections;
using UnityEngine;

public class AttackProjectileNode : BehaviourNode<BossController>
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
        Vector3 spawnPos = boss.transform.position + boss.transform.forward;

        GameObject sphere = boss.SpawnProjectile(spawnPos);

        Transform player = PlayerCombatController.Instance.transform;
        Vector3 dir = (player.position - spawnPos).normalized;
        sphere.GetComponent<Rigidbody>().AddForce(dir * boss.projectileForce, ForceMode.Impulse);

        yield return new WaitForSeconds(boss.attackCooldown);
        boss.currentAttack = -1;
        _running = false;
    }
}
