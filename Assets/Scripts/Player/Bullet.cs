using UnityEngine;

public class Bullet : MonoBehaviour
{
    GunData data;
    GameObject owner;
    float attack;
    float damageMultiplier;

    public void Initialize(GameObject owner, GunData data, float attack, float damageMultiplier = 1)
    {
        this.owner = owner;
        this.data = data;
        this.attack = attack;
        this.damageMultiplier = damageMultiplier;
        Destroy(gameObject, data.bulletLifetime);
    }

    void Update()
    {
        if (PauseController.IsPaused) return;
        transform.position += transform.forward * data.bulletSpeed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(owner.tag) || other.TryGetComponent<Bullet>(out _))
            return;

        float damage = data.damagePercent / 100 * attack * damageMultiplier;

        // Si la bala es del jugador
        if (other.TryGetComponent<Enemy>(out var enemy))
            enemy.TakeDamage(damage);

        // Si la bala es del jefe
        else if (other.TryGetComponent<PlayerCombatController>(out var player))
        {
            EnemyGunData bossData = (EnemyGunData)data;
            if (player.isIntangible) return;
            player.TakeDamage(damage, (player.transform.position - transform.position).normalized, bossData.knockbackDuration, bossData.intangibilityDuration, bossData.knockbackSpeed);
        }

        Destroy(gameObject);
    }
}
