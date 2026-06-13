using UnityEngine;

public class Bullet : MonoBehaviour
{
    float speed;
    GameObject owner;

    public void Initialize(GameObject owner, float speed, float lifetime)
    {
        this.owner = owner;
        this.speed = speed;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(owner.tag)) return;
        if (other.TryGetComponent<BossController>(out var boss))
            boss.TakeDamage(1);
        Destroy(gameObject);
    }
}
