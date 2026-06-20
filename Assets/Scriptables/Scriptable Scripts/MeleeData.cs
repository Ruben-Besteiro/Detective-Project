using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Data", menuName = "Game/Weapon/Melee")]
public class MeleeData : WeaponData
{
    public float rangeX;
    public float rangeY;
    public float rangeZ;
    public float hitboxOffset;
    public float stopDistance;
    public float activeDuration;    // Duración de la hitbox
    public float cooldown;
}