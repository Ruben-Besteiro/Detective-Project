using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
   [SerializeField] public EnemyData data;
   public float hp;
   
   void Start()
    {
        hp = data.hp;
    }

   public virtual void TakeDamage(float dmg)
   {
      hp -= dmg;
   }
}
