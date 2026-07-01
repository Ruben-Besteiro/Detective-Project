using UnityEngine;

public class Minion1Controller : Enemy
{
    [SerializeField] public EnemyMeleeData minionArmAttackData;

    private void OnEnable()
    {
        Boss1Controller.OnBossDied += OnBossKilled;
    }

    private void OnDisable()
    {
        Boss1Controller.OnBossDied -= OnBossKilled;
    }

    private void OnBossKilled()
        => TakeDamage(currentHp);
}