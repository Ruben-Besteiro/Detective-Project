using System.Collections;
using UnityEngine;

public class RangedAttackNode : BehaviourNode<Enemy>
{
    bool isMoving;
    float cooldown;

    public override State Start()
    {
        Debug.Log("RangedAttackNode");
        cooldown = ((BossController)ctx.agent).boss3BulletsAttackData.cooldown;
        isMoving = true;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update()
        => isMoving ? State.IN_PROGRESS : State.SUCCESS;

    IEnumerator Routine()
    {
        BossController enemy = (BossController)ctx.agent;
        if (PlayerCombatController.Instance == null) { isMoving = false; yield break; }
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
        enemy.currentAttack = -1;
        isMoving = false;
    }

    void FireProjectile(BossController enemy, Vector3 spawnPos, Vector3 dir)
    {
        GameObject sphere = enemy.Spawn(enemy.bulletPrefab, spawnPos);
        sphere.transform.rotation = Quaternion.LookRotation(dir);
    }
}
