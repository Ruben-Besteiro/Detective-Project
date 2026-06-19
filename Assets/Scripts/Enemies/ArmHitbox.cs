using UnityEngine;
using System.Collections;

// Me habría gustado usar OnControllerColliderHit, pero no funcionaba
public class ArmHitbox : MonoBehaviour
{
    [SerializeField] BossController boss;
    Collider col;

    void Awake()
    {
        col = GetComponent<Collider>();
        if (col is CapsuleCollider capsule)
        {
            BossMeleeData data = boss.bossMeleeData1;
            capsule.height = data.rangeY;
            capsule.radius = data.rangeX / 2f;
        }
    }

    void Update()
    {
        DebugBoxDrawer.DrawBox(col, new Color(1f, 0f, 0f, 0.5f), Time.deltaTime * 2f);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerCombatController.Instance.isIntangible) return;

        Debug.Log("Arm hitbox");
        Vector3 perpendicular = Vector3.Cross(transform.up, Vector3.up).normalized;

        // Elegir la dirección perpendicular que aleje al jugador del brazo
        Vector3 toPlayer = (other.transform.position - transform.position);
        if (Vector3.Dot(perpendicular, toPlayer) < 0)
            perpendicular = -perpendicular;

        PlayerCombatController.Instance.TakeDamage(boss.bossMeleeData1.damage, perpendicular, boss.bossMeleeData1.knockbackDuration, boss.bossMeleeData1.intangibilityDuration, boss.bossMeleeData1.knockbackSpeed);
    }
}
