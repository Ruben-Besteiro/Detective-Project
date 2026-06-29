using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AI;
using Pathfinding;

public class EnemyMovement : MonoBehaviour
{
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool succeeded;
    [HideInInspector] public float stopDistance;
    [HideInInspector] public float cooldown;

    private Enemy enemy;

    void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    public IEnumerator MoveEnemy()
    {
        Transform player = PlayerCombatController.Instance.transform;
        Vector3 dir = (player.position - enemy.transform.position).normalized;

        if (Physics.Raycast(enemy.transform.position, dir, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            yield return MoveDirectly(player);
        else
            yield return MoveWithPathfinding(player);

        succeeded = true;
        isMoving = false;
    }

    IEnumerator MoveDirectly(Transform player)
    {
        NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();
        agent.speed = enemy.stats.speed;
        agent.stoppingDistance = stopDistance;
        agent.updateRotation = false;
        agent.enabled = true;
        agent.Warp(enemy.transform.position);

        while (Vector3.Distance(enemy.transform.position, player.position) > stopDistance)
        {
            // Iniciamos el movimiento con NavMesh
            if (agent.isOnNavMesh)
                agent.SetDestination(player.position);
            enemy.LookAtPlayer();

            // Si hay una pared entre el enemigo y el jugador, pasamos a MoveWithPathfinding
            Vector3 dir = (new Vector3(player.position.x, enemy.transform.position.y, player.position.z) - enemy.transform.position).normalized;
            if (Physics.Raycast(enemy.transform.position, dir, out RaycastHit hitWall, Mathf.Infinity) && !hitWall.collider.CompareTag("Player"))
            {
                agent.enabled = false;
                yield return MoveWithPathfinding(player);
                yield break;
            }

            yield return null;
            if (PauseController.IsPaused)
            {
                agent.isStopped = true;
                yield return enemy.WaitWhilePaused();
                agent.isStopped = false;
            }
        }

        agent.enabled = false;
    }

    IEnumerator MoveWithPathfinding(Transform player)
    {
        Seeker seeker = enemy.GetComponent<Seeker>();
        List<Vector3> pathNodes = null;
        bool pathReady = false;

        seeker.StartPath(enemy.transform.position, player.position, (Path p) =>
        {
            if (!p.error)
                pathNodes = p.vectorPath;
            pathReady = true;
        });

        yield return new WaitUntil(() => pathReady);

        if (pathNodes == null || pathNodes.Count == 0)
        {
            succeeded = false;
            isMoving = false;
            yield break;
        }

        // Iniciamos el movimiento con A*
        int nodeIndex = 0;
        while (Vector3.Distance(enemy.transform.position, player.position) > stopDistance)
        {
            if (nodeIndex >= pathNodes.Count)
            {
                pathReady = false;
                seeker.StartPath(enemy.transform.position, player.position, (Path p) =>
                {
                    if (!p.error) pathNodes = p.vectorPath;
                    pathReady = true;
                });
                yield return new WaitUntil(() => pathReady);
                nodeIndex = 0;
                if (pathNodes == null || pathNodes.Count == 0) yield break;
            }

            Vector3 target = pathNodes[nodeIndex];
            target.y = enemy.transform.position.y;

            if (Vector3.Distance(enemy.transform.position, target) < 0.5f)
            {
                nodeIndex++;
                continue;
            }

            // Si hay línea directa al jugador, pasamos a MoveDirectly
            Vector3 dirToPlayer = (player.position - enemy.transform.position).normalized;
            if (Physics.Raycast(enemy.transform.position, dirToPlayer, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            {
                yield return MoveDirectly(player);
                yield break;
            }

            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target, enemy.stats.speed * Time.deltaTime);
            enemy.transform.forward = (target - enemy.transform.position).normalized;
            yield return null;
            yield return enemy.WaitWhilePaused();
        }
    }
}
