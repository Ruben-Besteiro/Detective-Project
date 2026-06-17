using UnityEngine;
using System.Collections;

// Me habría gustado usar OnControllerColliderHit, pero no funcionaba
public class ArmHitbox : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (PlayerCombatController.Instance.isIntangible) return;

        Vector3 perpendicular = Vector3.Cross(transform.up, Vector3.up).normalized;

        // Elegir la dirección perpendicular que aleje al jugador del brazo
        Vector3 toPlayer = (other.transform.position - transform.position);
        if (Vector3.Dot(perpendicular, toPlayer) < 0)
            perpendicular = -perpendicular;

        PlayerCombatController.Instance.TakeDamage(1, perpendicular, 0.5f);
    }
}
