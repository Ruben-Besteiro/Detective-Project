using UnityEngine;

[CreateAssetMenu(fileName = "PlayerStats", menuName = "Player/PlayerStats")]
public class PlayerStats : ScriptableObject
{
    [SerializeField] public string playerName;
    [SerializeField] public float speed;
    [SerializeField] public float maxHp;
}