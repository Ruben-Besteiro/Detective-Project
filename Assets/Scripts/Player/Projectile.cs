using UnityEngine;

public class Projectile : MonoBehaviour
{
    public void Initialize(float lifetime)
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player")) return;
        Debug.Log($"Proyectil impacta: {collision.gameObject.name}");
        Destroy(gameObject);
    }
}
