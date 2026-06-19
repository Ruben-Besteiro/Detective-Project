using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class BossController : Enemy
{
    [Header("Attacks")]
    [SerializeField] public BossGunData bossGunData1;       // Datos del ataque con balas
    [SerializeField] public BossMeleeData bossMeleeData1;   // Datos del ataque con el brazo
    public GameObject bulletPrefab;
    public float circleRadius = 3f;

    [HideInInspector] public bool startled = false;
    [HideInInspector] public int currentAttack = -1;
    
    [Header("Stuff")]
    [SerializeField] GameObject hud;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image lifeBar;

    BossBehaviourTree tree;

    void Awake()
    {
        tree = new BossBehaviourTree();
    }

    void Update()
    {
        if (PauseController.IsPaused) return;

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
                hud.SetActive(true);
                nameText.text = data.name;
                PlayerCombatController.Instance.playerNameText.text = PlayerCombatController.Instance.stats.playerName;
                tree.Start(this, this);
                break;
            }
        }
    }

    public void LookAtPlayer()
    {
        transform.LookAt(new Vector3(PlayerCombatController.Instance.transform.position.x, transform.position.y, PlayerCombatController.Instance.transform.position.z));
    }

    // Se llama desde AttackProjectileNode y AttackCircleNode
    public GameObject SpawnProjectile(Vector3 position)
    {
        if (bulletPrefab != null)
        {
            GameObject go = Instantiate(bulletPrefab, position, Quaternion.identity);
            go.GetComponent<Bullet>().Initialize(gameObject, bossGunData1);
            return go;
        }
        print("bulletPrefab is null");
        return null;
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
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
