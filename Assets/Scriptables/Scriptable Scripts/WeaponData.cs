using UnityEngine;

public abstract class WeaponData : ScriptableObject
{
    public string weaponName;
    public float damagePercent;     // El % de la stat de ataque que se transmite como daño
}