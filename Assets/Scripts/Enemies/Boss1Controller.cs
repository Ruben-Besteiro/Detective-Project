using UnityEngine;
using UnityEngine.Serialization;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class Boss1Controller : Enemy
{
    [Header("Attacks")]
    public EnemyGunData boss3BulletsAttackData;
    public EnemyMeleeData bossArmAttackData;
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
        nameText.text = stats.enemyName;
        if (PlayerCombatController.Instance != null)
            PlayerCombatController.Instance.playerNameText.text = PlayerCombatController.Instance.stats.playerName;
    }

    public override void TakeDamage(float dmg)
    {
        currentHp -= dmg;
        lifeBar.fillAmount = currentHp / stats.hp;
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
            Minion1Controller minion = m.GetComponent<Minion1Controller>();
            minion.TakeDamage(minion.currentHp);
        }
        Destroy(gameObject);
    }
}