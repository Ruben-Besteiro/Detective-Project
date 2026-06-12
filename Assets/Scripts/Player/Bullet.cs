using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed;

    public void Initialize(float speed, float lifetime)
    {
        this.speed = speed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        Debug.Log($"Proyectil impacta: {other.gameObject.name}");
        if (other.TryGetComponent<BossController>(out var boss))
        {
            boss.TakeDamage(1);
            Destroy(gameObject);
        }
    }
}
