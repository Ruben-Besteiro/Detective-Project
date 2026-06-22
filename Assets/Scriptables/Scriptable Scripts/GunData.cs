using UnityEngine;

[CreateAssetMenu(fileName = "New Gun Data", menuName = "Game/Weapon/Gun")]
public class GunData : WeaponData
{
    public float bulletSpeed;
    public float bulletLifetime;
    public float cooldown;
}
