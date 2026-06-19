using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Gun", menuName = "Boss/Boss Gun Data")]
public class BossGunData : GunData
{
    public float knockbackDuration;
    public float knockbackSpeed;
    public float intangibilityDuration;
}
