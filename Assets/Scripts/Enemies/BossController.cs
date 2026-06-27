using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class BossController : Enemy
{
    [Header("Attacks")]
    public BossGunData boss3BulletsAttackData;
    public BossMeleeData bossArmAttackData;
    public GameObject bulletPrefab;
    public GameObject minionPrefab;
    [HideInInspector] public List<GameObject> minionList;
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
        nameText.text = data.enemyName;
        if (PlayerCombatController.Instance != null)
            PlayerCombatController.Instance.playerNameText.text = PlayerCombatController.Instance.stats.playerName;
    }

    // Se llama desde RangedAttackNode y SpawnMinionsNode
    public GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab, position, Quaternion.identity);
            if (go.TryGetComponent<Bullet>(out Bullet b))
                b.Initialize(gameObject, boss3BulletsAttackData, 1);
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
        if (PlayerCombatController.Instance != null)
            PlayerCombatController.Instance.enabled = false;
        foreach (GameObject m in minionList)
        {
            if (m == null) continue;
            MinionController minion = m.GetComponent<MinionController>();
            minion.TakeDamage(minion.currentHp);
        }
        Destroy(gameObject);
    }
}