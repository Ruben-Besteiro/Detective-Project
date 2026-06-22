using UnityEngine;
using System.Collections;

public abstract class Enemy : MonoBehaviour
{
   [SerializeField] public EnemyData data;
   
   public float currentHp;
   [HideInInspector] public bool startled = false;

   protected BossBehaviourTree tree;
   [HideInInspector] public int currentAttack = -1;
   [HideInInspector] public bool lockRotation = false;

   public virtual BossMeleeData MeleeData => null;

   void Awake()
   {
      tree = new BossBehaviourTree();
   }

   protected virtual void Start()
   {
      currentHp = data.hp;
   }

   protected virtual void Update()
   {
      if (PauseController.IsPaused) return;

      if (!startled)
      {
         Collider[] hits = Physics.OverlapSphere(transform.position, data.startleRange);
         foreach (var hit in hits)
         {
            if (hit.CompareTag("Player")) { startled = true; OnStartled(); break; }
         }
      }

      if (startled)
      {
         if (!lockRotation) LookAtPlayer();
         tree.Start(this, this);
      }
   }

   protected virtual void OnStartled() { }

   // Llamamos a esto en los nodos del BT para retrasar los ataques hasta que despausemos
   public IEnumerator WaitWhilePaused()
   {
      while (PauseController.IsPaused) yield return null;
   }

   public void LookAtPlayer()
   {
      transform.LookAt(new Vector3(PlayerCombatController.Instance.transform.position.x, transform.position.y, PlayerCombatController.Instance.transform.position.z));
   }

   public virtual void TakeDamage(float dmg)
   {
      currentHp -= dmg;
      if (currentHp <= 0)
         Destroy(gameObject);
   }
}
