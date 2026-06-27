using UnityEngine;

public class Bullet : MonoBehaviour
{
    GunData data;
    GameObject owner;

    public void Initialize(GameObject owner, GunData data)
    {
        this.owner = owner;
        this.data = data;
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

        // Si la bala es del jugador
        if (other.TryGetComponent<Enemy>(out var enemy))
            enemy.TakeDamage(data.damage);

        // Si la bala es del jefe
        else if (other.TryGetComponent<PlayerCombatController>(out var player))
        {
            BossGunData bossData = (BossGunData)data;
            if (player.isIntangible) return;
            player.TakeDamage(bossData.damage, (player.transform.position - transform.position).normalized, bossData.knockbackDuration, bossData.intangibilityDuration, bossData.knockbackSpeed);
        }

        Destroy(gameObject);
    }
}
