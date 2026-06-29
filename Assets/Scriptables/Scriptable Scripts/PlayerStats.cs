using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [Header("Basic Stuff")]
    [SerializeField] public string playerName;
    [SerializeField] public float speed;
    [SerializeField] public float maxHp;

    [Header("Dash")]
    [SerializeField] public float dashSpeed;
    [SerializeField] public float dashDuration;
    [SerializeField] public float dashCooldown;

    [Header("Dodge")]
    [SerializeField] public float dodgeSpeed;
    [SerializeField] public float dodgeDuration;
    [SerializeField] public float dodgeCooldown;
}