using UnityEngine;

[CreateAssetMenu(fileName = "New Melee Data", menuName = "Game/Weapon/Melee")]
public class MeleeData : WeaponData
{
    public float rangeXY;
    public float rangeZ;
    public float hitboxOffset;
    public int activeFrames;
    public float endLag;
}