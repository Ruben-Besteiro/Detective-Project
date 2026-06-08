using System.Collections;
using UnityEngine;

public class AttackMoveNode : BehaviourNode<BossController>
{
    bool _running;
    bool _succeeded;

    public override State Start()
    {
        _running = true;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update() =>
        _running ? State.IN_PROGRESS : (_succeeded ? State.SUCCESS : State.FAILURE);

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
            _succeeded = false;
            _running = false;
            yield break;
        }

        while (Vector3.Distance(boss.transform.position, player.position) > boss.stopDistance)
        {
            dir = (player.position - boss.transform.position).normalized;
            boss.transform.position = Vector3.MoveTowards(
                boss.transform.position, player.position, boss.moveSpeed * Time.deltaTime);
            boss.transform.forward = dir;
            yield return null;
        }

        yield return new WaitForSeconds(boss.attackCooldown);
        boss.currentAttack = -1;
        _succeeded = true;
        _running = false;
    }
}
