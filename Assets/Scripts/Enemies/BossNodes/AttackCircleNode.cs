using System.Collections;
using UnityEngine;

public class AttackCircleNode : BehaviourNode<Enemy>
{
    bool isMoving;
    float cooldown = 2;
    int circleCount = 8;

    public override State Start()
    {
        Debug.Log("AttackCircleNode");
        isMoving = true;
        ctx.agent.StartCoroutine(Routine());
        return State.IN_PROGRESS;
    }

    public override State Update()
        => isMoving ? State.IN_PROGRESS : State.SUCCESS;

    IEnumerator Routine()
    {
        BossController enemy = (BossController)ctx.agent;
        enemy.LookAtPlayer();

        for (int i = 0; i < circleCount; i++)
        {
            float angle = i * Mathf.PI * 2f / circleCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * enemy.circleRadius;

            GameObject sphere = enemy.SpawnProjectile(enemy.transform.position + offset);
            // La fuerza debe ser hacia el jugador.
            Vector3 dir = (PlayerCombatController.Instance.transform.position - sphere.transform.position).normalized;
            sphere.GetComponent<Rigidbody>().AddForce(dir * 20, ForceMode.Impulse);
        }

        yield return new WaitForSeconds(cooldown);
        enemy.currentAttack = -1;
        isMoving = false;
    }
}
