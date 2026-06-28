using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Gun", menuName = "Enemy/Enemy Gun Data")]
public class EnemyGunData : GunData
{
    public float knockbackDuration;
    public float knockbackSpeed;
    public float intangibilityDuration;
}
