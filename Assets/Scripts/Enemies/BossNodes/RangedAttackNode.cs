using System.Collections;
using UnityEngine;

public class RangedAttackNode : BehaviourNode<Enemy>
{
    State result;
    float cooldown;

    public override State Start()
    {
        if (ctx.agent is Boss1Controller boss) cooldown = boss.boss3BulletsAttackData.cooldown;
        result = State.IN_PROGRESS;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() => result;

    IEnumerator Routine()
    {
        Enemy enemy = ctx.agent;
        if (PlayerCombatController.Instance == null)
        {
            result = State.FAILURE;
            yield break;
        }

        enemy.LookAtPlayer();

        Vector3 spawnPos = enemy.transform.position + enemy.transform.forward * (enemy.GetComponent<Collider>().bounds.extents.z + 1);
        spawnPos.y = 3;
        Vector3 baseDir = enemy.transform.forward;

        FireProjectile(enemy, spawnPos, baseDir);
        FireProjectile(enemy, spawnPos, Quaternion.AngleAxis(-15f, Vector3.up) * baseDir);
        FireProjectile(enemy, spawnPos, Quaternion.AngleAxis(15f, Vector3.up) * baseDir);

        float t = 0f;
        while (t < cooldown)
        {
            yield return null;
            yield return enemy.WaitWhilePaused();
            t += Time.deltaTime;
        }
        result = State.SUCCESS;
    }

    void FireProjectile(Enemy enemy, Vector3 spawnPos, Vector3 dir)
    {
        if (enemy is Boss1Controller boss) 
        {
            GameObject sphere = enemy.Spawn(boss.bulletPrefab, spawnPos);
            sphere.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}
