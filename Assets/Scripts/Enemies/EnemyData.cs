using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Data", menuName = "Game/Enemy Data")]
public class EnemyData : ScriptableObject
{
    [SerializeField] public string enemyName;
    [SerializeField] public float hp;
    [SerializeField] public float speed;
    [SerializeField] public float startleRange;     // La distancia a la que debe estar el jugador para que empiece a atacar
}
