using System.Collections;
using UnityEngine;

public class SpawnMinionsNode : BehaviourNode<Enemy>
{
    bool isMoving;
    int minionCount = 3;
    float cooldown = 4;

    public override State Start()
    {
        Debug.Log("SpawnMinionsNode");
        isMoving = true;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update()
        => isMoving ? State.IN_PROGRESS : State.SUCCESS;

    IEnumerator Routine()
    {
        Boss1Controller enemy = (Boss1Controller)ctx.agent;
        if (PlayerCombatController.Instance == null) { isMoving = false; yield break; }
        enemy.LookAtPlayer();

        for (int i = 0; i < minionCount; i++)
        {
            float angle = i * Mathf.PI * 2f / minionCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * enemy.minionSpawnRadius;
            float floorY = enemy.transform.position.y - enemy.GetComponent<Collider>().bounds.extents.y;
            Vector3 spawnPos = new Vector3(enemy.transform.position.x + offset.x, floorY, enemy.transform.position.z + offset.z);
            GameObject minion = enemy.Spawn(enemy.minionPrefab, spawnPos);
            float minionHalfHeight = minion.GetComponent<Collider>().bounds.extents.y;
            minion.transform.position = new Vector3(spawnPos.x, floorY + minionHalfHeight, spawnPos.z);
            minion.GetComponent<Enemy>().startled = true;
            enemy.minionList.Add(minion);
        }

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
}
