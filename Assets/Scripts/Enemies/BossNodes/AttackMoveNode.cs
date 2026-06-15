using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AttackMoveNode : BehaviourNode<BossController>
{
    bool isMoving;
    bool succeeded;
    float stopDistance;
    float cooldown = 3;
    BossController boss;

    public override State Start()
    {
        isMoving = true;
        stopDistance = Random.Range(0, 2) == 0 ? 10 : 15;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() =>
        isMoving ? State.IN_PROGRESS : (succeeded ? State.SUCCESS : State.FAILURE);

    IEnumerator Routine()
    {
        boss = ctx.agent;
        Transform player = PlayerCombatController.Instance.transform;
        Vector3 dir = (player.position - boss.transform.position).normalized;

        Debug.DrawRay(boss.transform.position, dir * (lineOfSight ? hit.distance : 50f),
                      lineOfSight ? Color.green : Color.red, 3f);

        if (Physics.Raycast(boss.transform.position, dir, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            yield return MoveDirectly(boss, player);
        else
            yield return MoveWithPathfinding(boss, player);

        yield return new WaitForSeconds(cooldown);
        boss.currentAttack = -1;
        succeeded = true;
        isMoving = false;
    }

    IEnumerator MoveDirectly(BossController boss, Transform player)
    {
        Debug.Log("Raycast dio verdadero => Nos movemos directamente hacia el jugador");
        while (Vector3.Distance(boss.transform.position, player.position) > stopDistance)
        {
            Vector3 dir = (player.position - boss.transform.position).normalized;
            Debug.DrawRay(boss.transform.position, dir * (stillVisible ? hit.distance : 50f),
                          stillVisible ? Color.green : Color.red, 0.1f);

            if (!Physics.Raycast(boss.transform.position, dir, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            {
                Debug.Log("Perdimos la línea de visión => Cambiamos a pathfinding");
                yield return MoveWithPathfinding(boss, player);
                yield break;
            }

            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, player.position, boss.data.speed * Time.deltaTime);
            boss.transform.forward = dir;
            yield return null;
        }
    }

    IEnumerator MoveWithPathfinding(BossController boss, Transform player)
    {
        Debug.Log("Raycast dio falso => Buscamos ruta con pathfinding");
        Seeker seeker = boss.GetComponent<Seeker>();
        List<Vector3> pathNodes = null;
        bool pathReady = false;

        seeker.StartPath(boss.transform.position, player.position, (Path p) =>
        {
            if (!p.error)
            {
                pathNodes = p.vectorPath;
            }
            pathReady = true;
        });

        yield return new WaitUntil(() => pathReady);

        if (pathNodes == null || pathNodes.Count == 0)
        {
            succeeded = false;
            isMoving = false;
            yield break;
        }

        int nodeIndex = 0;
        while (Vector3.Distance(boss.transform.position, player.position) > stopDistance)
        {
            // Recalcular ruta periódicamente si el jugador se mueve
            if (nodeIndex >= pathNodes.Count)
            {
                pathReady = false;
                seeker.StartPath(boss.transform.position, player.position, (Path p) =>
                {
                    if (!p.error) pathNodes = p.vectorPath;
                    pathReady = true;
                });
                yield return new WaitUntil(() => pathReady);
                nodeIndex = 0;
                if (pathNodes == null || pathNodes.Count == 0) yield break;
            }

            Vector3 target = pathNodes[nodeIndex];
            target.y = boss.transform.position.y;

            if (Vector3.Distance(boss.transform.position, target) < 0.5f)
            {
                nodeIndex++;
                continue;
            }

            Vector3 dirToPlayer = (player.position - boss.transform.position).normalized;
            Debug.DrawRay(boss.transform.position, dirToPlayer * (lineOfSight ? hit.distance : 50f),
                          lineOfSight ? Color.green : Color.red, 0.1f);

            if (Physics.Raycast(boss.transform.position, dirToPlayer, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            {
                Debug.Log("Recuperamos línea de visión => Cambiamos a movimiento directo");
                yield return MoveDirectly(boss, player);
                yield break;
            }

            Vector3 dir = (target - boss.transform.position).normalized;
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, target, boss.data.speed * Time.deltaTime);
            boss.transform.forward = dir;
            yield return null;
        }
    }
}
