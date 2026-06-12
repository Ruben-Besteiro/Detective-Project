using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
   public void TakeDamage(float dmg)
   {
      print("ÑÑÑÑÑÑÑÑÑÑÑÑÑÑÑÑÑÑÑÑ");
      Destroy(gameObject);
   }
}
