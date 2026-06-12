using UnityEngine;

public class BossController : Enemy
{
    [Header("Detection")]
    public float detectionRadius = 20f;

    [Header("Movement")]
    public float moveSpeed = 4f;
    public float stopDistance = 5f;

    [Header("Attacks")]
    public float attackCooldown = 1.5f;
    public GameObject projectilePrefab;
    public float projectileForce = 12f;
    public int circleCount = 8;
    public float circleRadius = 3f;

    [HideInInspector] public bool startled;
    [HideInInspector] public int currentAttack = -1;

    BossBehaviourTree _tree; 

    void Awake()
    {
        _tree = new BossBehaviourTree();
    }

    void Update()
    {
        if (startled) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                startled = true;
                _tree.Start(this, this);
                break;
            }
        }
    }

    public GameObject SpawnProjectile(Vector3 position)
    {
        if (projectilePrefab != null)
            return Instantiate(projectilePrefab, position, Quaternion.identity);

        var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        go.transform.position = position;
        go.transform.localScale = Vector3.one * 0.4f;
        if (go.GetComponent<Rigidbody>() == null)
            go.AddComponent<Rigidbody>();
        return go;
    }
}
