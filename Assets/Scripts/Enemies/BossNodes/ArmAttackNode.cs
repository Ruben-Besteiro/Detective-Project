using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmAttackNode : BehaviourNode<Enemy>
{
    State result;
    bool mediumRange;
    float stopDistance;
    GameObject selectedArm;
    float cooldown;
    Enemy enemy;

    EnemyMeleeData data;

    public override State Start()
    {
        enemy = ctx.agent;
        result = State.IN_PROGRESS;
        mediumRange = UnityEngine.Random.Range(0, 2) == 0;

        if (enemy is Boss1Controller boss)
            data = boss.bossArmAttackData;
        else if (enemy is Minion1Controller minion)
            data = minion.minionArmAttackData;

        cooldown = data.cooldown;
        stopDistance = data.stopDistance;
        if (mediumRange) stopDistance *= 1.5f;

        string armName = UnityEngine.Random.Range(0, 2) == 0 ? "Arm L" : "Arm R";
        Transform armTransform = enemy.transform.Find(armName);
        selectedArm = armTransform != null ? armTransform.gameObject : null;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() => result;

    IEnumerator Routine()
    {
        enemy = ctx.agent;
        if (PlayerCombatController.Instance == null)
        {
            result = State.FAILURE;
            yield break;
        }

        enemy.stopDistance = stopDistance;
        yield return enemy.StartCoroutine(enemy.MoveEnemy());

        if (!enemy.succeeded)
        {
            result = State.FAILURE;
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

        result = State.SUCCESS;
    }

    IEnumerator SweepAttack(Enemy enemy, Transform player)
    {
        if (selectedArm == null) yield break;
        Transform arm = selectedArm.transform;
        bool isLeft = selectedArm.name == "Arm L";

        Vector3 originalScale    = arm.localScale;
        Vector3 originalPosition = arm.localPosition;
        Quaternion originalRot   = arm.localRotation;

        arm.localScale = new Vector3(originalScale.x, originalScale.y * (mediumRange ? 3 : 2), originalScale.z);
        float halfLen = arm.localScale.y;

        enemy.LookAtPlayer();
        enemy.lockRotation = true;

        float sweepDuration;
        Action<float> updateSweep;      // Esto es un delegate = una función que se guarda en una variable

        // Según qué enemigo haga el ataque, hay cosas que cambian
        if (enemy is Boss1Controller)
        {
            sweepDuration = 1f;
            arm.localRotation = Quaternion.Euler(0, isLeft ? 180 : 0, isLeft ? 90 : -90);
            arm.localPosition = originalPosition + (isLeft ? Vector3.left : Vector3.right) * halfLen;
            float startAngle   = enemy.transform.eulerAngles.y;
            float rotationSign = isLeft ? 1f : -1f;

            // Aquí relleno el delegate con cómo debe ser el movimiento del brazo del jefe
            updateSweep = t =>
            {
                enemy.transform.rotation = Quaternion.Euler(0, startAngle + 180f * rotationSign * t, 0);
            };
        }
        else
        {
            sweepDuration = 0.5f;
            Quaternion startRot = Quaternion.Euler(-45f, 0f, 0f);
            Quaternion endRot = Quaternion.Euler( 90f, 0f, 0f);
            arm.localRotation = startRot;
            arm.localPosition = originalPosition + startRot * Vector3.up * halfLen;

            // Y aquí lo mismo pero para el minion
            updateSweep = t =>
            {
                arm.localRotation = Quaternion.Slerp(startRot, endRot, t);
                arm.localPosition = originalPosition + arm.localRotation * Vector3.up * halfLen;
            };
        }

        selectedArm.GetComponent<Collider>().enabled = true;
        float elapsed = 0f;

        while (elapsed < sweepDuration)
        {
            elapsed += Time.deltaTime;
            updateSweep(Mathf.Clamp01(elapsed / sweepDuration));
            if (elapsed > data.activeDuration)
                selectedArm.GetComponent<Collider>().enabled = false;
            yield return null;
            yield return enemy.WaitWhilePaused();
        }

        enemy.lockRotation = false;
        arm.localScale = originalScale;
        arm.localPosition = originalPosition;
        arm.localRotation = originalRot;
    }
}
