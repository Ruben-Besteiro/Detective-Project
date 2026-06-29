using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmAttackNode : BehaviourNode<Enemy>
{
    bool isMoving;
    bool succeeded;
    bool mediumRange;
    float stopDistance;
    GameObject selectedArm;
    float cooldown;
    Enemy enemy;

    EnemyMeleeData data;

    public override State Start()
    {
        enemy = ctx.agent;
        isMoving = true;
        mediumRange = Random.Range(0, 2) == 0;

        if (enemy is Boss1Controller boss)
            data = boss.bossArmAttackData;
        else if (enemy is Minion1Controller minion)
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
        if (PlayerCombatController.Instance == null)
        {
            isMoving = false;
            yield break;
        }

        var movement = enemy.GetComponent<EnemyMovement>();
        movement.stopDistance = stopDistance;
        movement.cooldown = cooldown;
        movement.isMoving = true;
        movement.succeeded = false;
        yield return enemy.StartCoroutine(movement.MoveEnemy());

        if (!movement.succeeded)
        {
            isMoving = false;
            yield break;
        }

        if (selectedArm != null)
            yield return SweepAttack(enemy, PlayerCombatController.Instance.transform);

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
        arm.localScale = new Vector3(originalScale.x, originalScale.y * (mediumRange ? 3 : 2), originalScale.z);

        if (enemy is Minion1Controller)
        {
            sweepDuration = 0.5f;
            // Golpe de arriba a abajo pivotando desde el hombro
            enemy.LookAtPlayer();
            enemy.lockRotation = true;

            // Semilongitud del brazo escalado en espacio local del enemigo
            float halfLen = arm.localScale.y;
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

            // Hombro fijo en originalPosition; el centro del brazo se desplaza según la longitud escalada
            float halfLen = arm.localScale.y;
            Vector3 outward = isLeft ? Vector3.left : Vector3.right;
            arm.localPosition = originalPosition + outward * halfLen;
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
