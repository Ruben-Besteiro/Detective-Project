using UnityEngine;

[CreateAssetMenu(fileName = "New Boss Melee", menuName = "Boss/Boss Melee Data")]
public class BossMeleeData : MeleeData
{
    public float knockbackDuration;
    public float knockbackSpeed;
    public float intangibilityDuration;
}
