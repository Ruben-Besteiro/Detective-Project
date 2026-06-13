using System.Collections;
using UnityEngine;

public class AttackMoveNode : BehaviourNode<BossController>
{
    bool isMoving;
    bool succeeded;
    float stopDistance;
    float cooldown = 3;

    public override State Start()
    {
        isMoving = true;
        stopDistance = Random.Range(0, 2) == 0 ? 10 : 15;        // Corta o media distancia
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() =>
        isMoving ? State.IN_PROGRESS : (succeeded ? State.SUCCESS : State.FAILURE);

    IEnumerator Routine()
    {
        BossController boss = ctx.agent;
        Transform player = PlayerCombatController.Instance.transform;
        Vector3 dir = (player.position - boss.transform.position).normalized;

        if (!Physics.Raycast(boss.transform.position, dir, out RaycastHit hit, Mathf.Infinity)
            || !hit.collider.CompareTag("Player"))
        {
            Debug.Log("El movimiento no es posible");
            boss.currentAttack = -1;
            succeeded = false;
            isMoving = false;
            yield break;
        }

        while (Vector3.Distance(boss.transform.position, player.position) > stopDistance)
        {
            dir = (player.position - boss.transform.position).normalized;
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, player.position, boss.data.speed * Time.deltaTime);
            boss.transform.forward = dir;
            yield return null;
        }

        yield return new WaitForSeconds(cooldown);
        boss.currentAttack = -1;
        succeeded = true;
        isMoving = false;
    }
}
