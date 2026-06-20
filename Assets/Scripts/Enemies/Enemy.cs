using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
   [SerializeField] public EnemyData data;
   
   [HideInInspector] public float currentHp;
   [HideInInspector] public bool startled = false;

   protected BossBehaviourTree tree;
   [HideInInspector] public int currentAttack = -1;

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
         LookAtPlayer();
         tree.Start(this, this);
      }
   }

   protected virtual void OnStartled() { }

   public void LookAtPlayer()
   {
      transform.LookAt(new Vector3(PlayerCombatController.Instance.transform.position.x, transform.position.y, PlayerCombatController.Instance.transform.position.z));
   }

   public virtual void TakeDamage(float dmg)
   {
      currentHp -= dmg;
   }
}
