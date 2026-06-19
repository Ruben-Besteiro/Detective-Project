using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AttackMoveNode : BehaviourNode<BossController>
{
    bool isMoving;
    bool succeeded;
    bool mediumRange;
    float stopDistance;
    GameObject selectedArm;
    float cooldown;
    BossController boss;

    public override State Start()
    {
        boss = ctx.agent;
        isMoving = true;
        mediumRange = Random.Range(0, 2) == 0;
        cooldown = boss.bossMeleeData1.cooldown;

        if (mediumRange) stopDistance = 15;
        else stopDistance = 10;

        if (Random.Range(0, 2) == 0)
            selectedArm = GameObject.Find("Arm L");
        else
            selectedArm = GameObject.Find("Arm R");
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

        if (Physics.Raycast(boss.transform.position, dir, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
            yield return MoveDirectly(boss, player);
        else
            yield return MoveWithPathfinding(boss, player);

        yield return SweepAttack(boss, player);
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
            boss.LookAtPlayer();
            Vector3 flatTarget = new Vector3(player.position.x, boss.transform.position.y, player.position.z);
            Vector3 dir = (flatTarget - boss.transform.position).normalized;

            if (Physics.Raycast(boss.transform.position, dir, out RaycastHit hit, Mathf.Infinity))
            {
                if (!hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Perdimos la línea de visión => Cambiamos a pathfinding");
                    yield return MoveWithPathfinding(boss, player);
                    yield break;
                }
            }

            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, flatTarget, boss.data.speed * Time.deltaTime);
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

            if (Physics.Raycast(boss.transform.position, dirToPlayer, out RaycastHit hit, Mathf.Infinity))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("Recuperamos línea de visión => Cambiamos a movimiento directo");
                    yield return MoveDirectly(boss, player);
                    yield break;
                }
            }

            Vector3 dir = (target - boss.transform.position).normalized;
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, target, boss.data.speed * Time.deltaTime);
            boss.transform.forward = dir;
            yield return null;
        }
    }

    IEnumerator SweepAttack(BossController boss, Transform player)
    {
        Transform arm = selectedArm.transform;
        bool isLeft = selectedArm.name == "Arm L";

        Vector3 originalScale = arm.localScale;
        Vector3 originalPosition = arm.localPosition;
        Quaternion originalRot = arm.localRotation;

        // Apuntar el brazo totalmente a la izquierda o derecha
        // La cápsula tiene su eje de altura en Y local; rotando Z±90° lo alineamos con X
        arm.localRotation = Quaternion.Euler(0, isLeft ? 180 : 0, isLeft ? 90 : -90);

        // Escalar para cubrir la stopping distance (altura de cápsula = 2 * scaleY)
        if (mediumRange) arm.localScale = new Vector3(originalScale.x, originalScale.y * 4.5f, originalScale.z);
        else arm.localScale = new Vector3(originalScale.x, originalScale.y * 3, originalScale.z);

        // Centrar el brazo en la dirección correcta
        arm.localPosition = new Vector3(isLeft ? -1 : 1, originalPosition.y, 0);

        // Mirar al jugador antes de barrer
        boss.LookAtPlayer();

        // Girar 180° gradualmente
        float sweepDuration = 1;        // Duración total del ataque excepto el cooldown
        float elapsed = 0;
        float startAngle = boss.transform.eulerAngles.y;

        selectedArm.GetComponent<Collider>().enabled = true;

        while (elapsed < sweepDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / sweepDuration);
            boss.transform.rotation = Quaternion.Euler(0, startAngle + (isLeft ? 180 : -180) * t, 0);

            if (elapsed > boss.bossMeleeData1.activeDuration)
                selectedArm.GetComponent<Collider>().enabled = false;
            yield return null;
        }

        // Restaurar brazo
        arm.localScale = originalScale;
        arm.localPosition = originalPosition;
        arm.localRotation = originalRot;
    }
}
