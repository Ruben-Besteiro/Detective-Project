using System.Collections;
using UnityEngine;

public class AttackProjectileNode : BehaviourNode<BossController>
{
    bool isMoving;
    float cooldown;

    public override State Start()
    {
        Debug.Log("AttackProjectileNode");
        cooldown = ctx.agent.bossGunData1.cooldown;
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

        Vector3 spawnPos = boss.transform.position + boss.transform.forward * (boss.GetComponent<Collider>().bounds.extents.z + 1);
        spawnPos.y = 3;
        Vector3 baseDir = boss.transform.forward;

        FireProjectile(boss, spawnPos, baseDir);
        FireProjectile(boss, spawnPos, Quaternion.AngleAxis(-15f, Vector3.up) * baseDir);
        FireProjectile(boss, spawnPos, Quaternion.AngleAxis(15f, Vector3.up) * baseDir);

        yield return new WaitForSeconds(cooldown);
        boss.currentAttack = -1;
        isMoving = false;
    }

    void FireProjectile(BossController boss, Vector3 spawnPos, Vector3 dir)
    {
        GameObject sphere = boss.SpawnProjectile(spawnPos);
        sphere.transform.rotation = Quaternion.LookRotation(dir);
    }
}
