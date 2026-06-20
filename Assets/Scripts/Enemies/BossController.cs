using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;

public class BossController : Enemy
{
    [Header("Attacks")]
    [SerializeField] public BossGunData boss3BulletsAttackData;
    [SerializeField] public BossMeleeData bossArmAttackData;
    public GameObject bulletPrefab;
    public float circleRadius = 3f;

    [Header("Stuff")]
    [SerializeField] GameObject hud;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] Image lifeBar;

    public override BossMeleeData MeleeData => bossArmAttackData;

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
    public GameObject SpawnProjectile(Vector3 position)
    {
        if (bulletPrefab != null)
        {
            GameObject go = Instantiate(bulletPrefab, position, Quaternion.identity);
            go.GetComponent<Bullet>().Initialize(gameObject, boss3BulletsAttackData);
            return go;
        }
        print("bulletPrefab is null");
        return null;
    }

    public override void TakeDamage(float dmg)
    {
        base.TakeDamage(dmg);
        lifeBar.fillAmount = currentHp / data.hp;
        if (currentHp <= 0)
            Die();
    }

    void Die()
    {
        PlayerCombatController.Instance.enabled = false;
        Destroy(gameObject);
    }
}
