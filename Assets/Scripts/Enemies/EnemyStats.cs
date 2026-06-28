using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Stats", menuName = "Game/Enemy Stats")]
public class EnemyStats : ScriptableObject
{
    [SerializeField] public string enemyName;
    [SerializeField] public float hp;
    [SerializeField] public float speed;
    [SerializeField] public float startleRange;     // La distancia a la que debe estar el jugador para que empiece a atacar
}
