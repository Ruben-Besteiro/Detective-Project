using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Melee", menuName = "Enemy/Enemy Melee Data")]
public class EnemyMeleeData : MeleeData
{
    public float knockbackDuration;
    public float knockbackSpeed;
    public float intangibilityDuration;
}
