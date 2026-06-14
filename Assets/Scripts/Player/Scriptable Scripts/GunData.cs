using UnityEngine;

[CreateAssetMenu(fileName = "New Gun Data", menuName = "Game/Weapon/Gun")]
public class GunData : WeaponData
{
    public GameObject bulletPrefab;
    public float bulletSpeed;
    public float bulletLifetime;
    public float cooldown;
}
