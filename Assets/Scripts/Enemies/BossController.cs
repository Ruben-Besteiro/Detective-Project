using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossController : Enemy
{
    [Header("Attacks")]
    public GameObject bulletPrefab;
    public float projectileForce = 12f;
    public int circleCount = 8;
    public float circleRadius = 3f;

    [HideInInspector] public bool startled = false;
    [HideInInspector] public int currentAttack = -1;
    
    [Header("Stuff")]
    [SerializeField] GameObject bossCanvas;

    BossBehaviourTree tree;

    void Awake()
    {
        tree = new BossBehaviourTree();
    }

    void Update()
    {
        if (startled)
        {
            transform.LookAt(new Vector3(PlayerCombatController.Instance.transform.position.x, transform.position.y, PlayerCombatController.Instance.transform.position.z));
            return;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, data.startleRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                startled = true;
                bossCanvas.SetActive(true);
                bossCanvas.GetComponentInChildren<TextMeshProUGUI>().text = data.name;
                tree.Start(this, this);
                break;
            }
        }
    }

    // Se llama desde AttackProjectileNode y AttackCircleNode
    public GameObject SpawnProjectile(Vector3 position)
    {
        if (bulletPrefab != null)
        {
            GameObject go = Instantiate(bulletPrefab, position, Quaternion.identity);
            go.GetComponent<Bullet>().Initialize(gameObject, 10, 5);
            return go;
        }
        return null;
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        Image lifeBar = bossCanvas.GetComponentsInChildren<Image>()[2];
        lifeBar.fillAmount = hp / data.hp;
        if (hp <= 0)
            Die();
    }

    void Die()
    {
        PlayerCombatController.Instance.enabled = false;
        // Animación de muerte
        Destroy(gameObject);
    }
}
