using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AttackMoveNode : BehaviourNode<Enemy>
{
    bool isMoving;
    bool succeeded;
    bool mediumRange;
    float stopDistance;
    GameObject selectedArm;
    float cooldown;
    Enemy enemy;

    BossMeleeData data;

    public override State Start()
    {
        enemy = ctx.agent;
        isMoving = true;
        mediumRange = Random.Range(0, 2) == 0;

        if (enemy is BossController boss)
            data = boss.bossArmAttackData;
        else if (enemy is MinionController minion)
            data = minion.minionArmAttackData;

        cooldown = data.cooldown;
        stopDistance = data.stopDistance;
        if (mediumRange) stopDistance *= 1.5f;

        string armName = Random.Range(0, 2) == 0 ? "Arm L" : "Arm R";
        Transform armTransform = enemy.transform.Find(armName);
        selectedArm = armTransform != null ? armTransform.gameObject : null;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() =>
        isMoving ? State.IN_PROGRESS : (succeeded ? State.SUCCESS : State.FAILURE);

    IEnumerator Routine()
    {
        enemy = ctx.agent;
        Transform player = PlayerCombatController.Instance.transform;
        Vector3 dir = (player.position - enemy.transform.position).normalized;

        if (Physics.Raycast(enemy.transform.position, dir, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            yield return MoveDirectly(enemy, player);
        else
            yield return MoveWithPathfinding(enemy, player);

        if (selectedArm != null)
            yield return SweepAttack(enemy, player);

        float t = 0f;
        while (t < cooldown)
        {
            yield return null;
            yield return enemy.WaitWhilePaused();
            t += Time.deltaTime; 
        }
        enemy.currentAttack = -1;
        succeeded = true;
        isMoving = false;
    }

    IEnumerator MoveDirectly(Enemy enemy, Transform player)
    {
        while (Vector3.Distance(enemy.transform.position, player.position) > stopDistance)
        {
            enemy.LookAtPlayer();
            Vector3 flatTarget = new Vector3(player.position.x, enemy.transform.position.y, player.position.z);
            Vector3 dir = (flatTarget - enemy.transform.position).normalized;

            if (Physics.Raycast(enemy.transform.position, dir, out RaycastHit hit, Mathf.Infinity))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    yield return MoveWithPathfinding(enemy, player);
                    yield break;
                }
            }

            enemy.transform.position = Vector3.MoveTowards(
                enemy.transform.position, flatTarget, enemy.data.speed * Time.deltaTime);
            enemy.transform.forward = dir;
            yield return null;
            yield return enemy.WaitWhilePaused();
        }
    }

    IEnumerator MoveWithPathfinding(Enemy enemy, Transform player)
    {
        Seeker seeker = enemy.GetComponent<Seeker>();
        List<Vector3> pathNodes = null;
        bool pathReady = false;

        seeker.StartPath(enemy.transform.position, player.position, (Path p) =>
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
        while (Vector3.Distance(enemy.transform.position, player.position) > stopDistance)
        {
            // Recalcular ruta periódicamente si el jugador se mueve
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

            Vector3 dirToPlayer = (player.position - enemy.transform.position).normalized;

            if (Physics.Raycast(enemy.transform.position, dirToPlayer, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    yield return MoveDirectly(enemy, player);
                    yield break;
                }
            }

            Vector3 dir = (target - enemy.transform.position).normalized;
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, target, enemy.data.speed * Time.deltaTime);
            enemy.transform.forward = dir;
            yield return null;
            yield return enemy.WaitWhilePaused();
        }
    }

    IEnumerator SweepAttack(Enemy enemy, Transform player)
    {
        if (selectedArm == null) yield break;
        Transform arm = selectedArm.transform;
        bool isLeft = selectedArm.name == "Arm L";

        Vector3 originalScale = arm.localScale;
        Vector3 originalPosition = arm.localPosition;
        Quaternion originalRot = arm.localRotation;

        float sweepDuration;
        float elapsed = 0f;

        if (enemy is MinionController)
        {
            sweepDuration = 0.5f;
            // Golpe de arriba a abajo pivotando desde el hombro
            enemy.LookAtPlayer();
            enemy.lockRotation = true;
            arm.localScale = new Vector3(originalScale.x, originalScale.y * (mediumRange ? 4f : 3f), originalScale.z);

            // Semilongitud del brazo escalado en espacio local del enemigo
            float halfLen = data.rangeY / 2f * arm.localScale.y;
            // Hombro: extremo fijo en espacio local del enemigo
            Vector3 shoulderLocal = originalPosition;

            Quaternion startRot = Quaternion.Euler(-45f, 0f, 0f);
            Quaternion endRot   = Quaternion.Euler( 90f, 0f, 0f);
            arm.localRotation = startRot;
            // El centro del transform se coloca a halfLen desde el hombro en la dirección del brazo
            arm.localPosition = shoulderLocal + startRot * Vector3.up * halfLen;

            selectedArm.GetComponent<Collider>().enabled = true;
            while (elapsed < sweepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / sweepDuration);
                arm.localRotation = Quaternion.Slerp(startRot, endRot, t);
                // Mantener el hombro fijo actualizando la posición del centro
                arm.localPosition = shoulderLocal + arm.localRotation * Vector3.up * halfLen;
                if (elapsed > data.activeDuration)
                    selectedArm.GetComponent<Collider>().enabled = false;
                yield return null;
                yield return enemy.WaitWhilePaused();
            }
        }
        else
        {
            sweepDuration = 1;
            // El jefe barre en un semicírculo
            arm.localRotation = Quaternion.Euler(0, isLeft ? 180 : 0, isLeft ? 90 : -90);

            if (mediumRange) arm.localScale = new Vector3(originalScale.x, originalScale.y * 4.5f, originalScale.z);
            else arm.localScale = new Vector3(originalScale.x, originalScale.y * 3, originalScale.z);

            arm.localPosition = new Vector3(isLeft ? -1 : 1, originalPosition.y, 0);
            enemy.LookAtPlayer();
            enemy.lockRotation = true;

            float startAngle = enemy.transform.eulerAngles.y;
            selectedArm.GetComponent<Collider>().enabled = true;
            while (elapsed < sweepDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / sweepDuration);
                enemy.transform.rotation = Quaternion.Euler(0, startAngle + (isLeft ? 180 : -180) * t, 0);
                if (elapsed > data.activeDuration)
                    selectedArm.GetComponent<Collider>().enabled = false;
                yield return null;
                yield return enemy.WaitWhilePaused();
            }
        }

        enemy.lockRotation = false;

        // Restaurar brazo
        arm.localScale = originalScale;
        arm.localPosition = originalPosition;
        arm.localRotation = originalRot;
    }
}
