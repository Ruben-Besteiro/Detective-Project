using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;

public class BossController : Enemy
{
    [Header("Attacks")]
    public BossGunData boss3BulletsAttackData;
    public BossMeleeData bossArmAttackData;
    public GameObject bulletPrefab;
    public GameObject minionPrefab;
    public float minionSpawnRadius;

    [Header("Stuff")]
    [SerializeField] private GameObject hud;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private Image lifeBar;

    protected override void Start()
    {
        base.Start();
        foreach (var i in GetComponentsInChildren<CapsuleCollider>())
            i.enabled = false;
    }

    protected override void OnStartled()
    {
        hud.SetActive(true);
        nameText.text = data.name;
        PlayerCombatController.Instance.playerNameText.text = PlayerCombatController.Instance.stats.playerName;
    }

    // Se llama desde AttackProjectileNode y AttackCircleNode
    public GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab, position, Quaternion.identity);
            if (go.TryGetComponent<Bullet>(out Bullet b))
                b.Initialize(gameObject, boss3BulletsAttackData);
            return go;
        }
        return null;
    }

    public override void TakeDamage(float dmg)
    {
        currentHp -= dmg;
        lifeBar.fillAmount = currentHp / data.hp;
        if (currentHp <= 0)
            Die();
    }

    private void Die()
    {
        PlayerCombatController.Instance.enabled = false;
        foreach (var minion in FindObjectsOfType<MinionController>())
            minion.TakeDamage(minion.currentHp);
        Destroy(gameObject);
    }
}
