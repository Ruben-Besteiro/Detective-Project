using System.Collections;
using UnityEngine;

public class AttackProjectileNode : BehaviourNode<BossController>
{
    bool isMoving;
    float cooldown = 2;

    public override State Start()
    {
        isMoving = true;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update()
        => isMoving ? State.IN_PROGRESS : State.SUCCESS;

    IEnumerator Routine()
    {
        BossController boss = ctx.agent;
        boss.LookAtPlayer();

        Vector3 spawnPos = boss.transform.position + boss.transform.forward;

        GameObject sphere = boss.SpawnProjectile(spawnPos);

        Transform player = PlayerCombatController.Instance.transform;
        Vector3 dir = (player.position - spawnPos).normalized;
        sphere.GetComponent<Rigidbody>().AddForce(dir * boss.projectileForce, ForceMode.Impulse);

        yield return new WaitForSeconds(cooldown);
        boss.currentAttack = -1;
        isMoving = false;
    }
}
