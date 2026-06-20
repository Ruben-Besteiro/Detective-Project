using UnityEngine;

// Me habría gustado usar OnControllerColliderHit, pero no funcionaba
public class ArmHitbox : MonoBehaviour
{
    [SerializeField] Enemy enemy;
    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col is CapsuleCollider capsule && enemy.MeleeData != null && enemy.MeleeData.rangeX > 0)
        {
            capsule.height = enemy.MeleeData.rangeY;
            capsule.radius = enemy.MeleeData.rangeX / 2f;
        }
    }

    void Update()
    {
        if (col.enabled)
            DebugBoxDrawer.DrawBox(col, new Color(1f, 0f, 0f, 0.5f), Time.deltaTime * 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerCombatController.Instance.isIntangible) return;

        Debug.Log("Arm hitbox");
        Vector3 perpendicular = Vector3.Cross(transform.up, Vector3.up).normalized;

        Vector3 toPlayer = other.transform.position - transform.position;
        if (Vector3.Dot(perpendicular, toPlayer) < 0)
            perpendicular = -perpendicular;

        // Hacemos el daño según qué scriptable tenga el enemigo
        if (enemy is BossController boss)
            PlayerCombatController.Instance.TakeDamage(boss.bossArmAttackData.damage, perpendicular, boss.bossArmAttackData.knockbackDuration, boss.bossArmAttackData.intangibilityDuration, boss.bossArmAttackData.knockbackSpeed);
        else if (enemy is MinionController minion)
            PlayerCombatController.Instance.TakeDamage(minion.minionArmAttackData.damage, perpendicular, minion.minionArmAttackData.knockbackDuration, minion.minionArmAttackData.intangibilityDuration, minion.minionArmAttackData.knockbackSpeed);
    }
}
