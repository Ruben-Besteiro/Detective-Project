using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
using Pathfinding;

public abstract class Enemy : MonoBehaviour
{
   [SerializeField] public EnemyStats stats;
   
   public float currentHp;
   [HideInInspector] public bool startled = false;

   protected BossBehaviourTree tree;
   [HideInInspector] public int currentAttack = -1;
   [HideInInspector] public bool lockRotation = false;

   [HideInInspector] public bool isMoving;
   [HideInInspector] public bool succeeded;
   [HideInInspector] public float stopDistance;
   [HideInInspector] public float cooldown;

   public virtual EnemyMeleeData MeleeData => null;

   void Awake()
   {
      tree = new BossBehaviourTree();
   }

   protected virtual void Start()
   {
      currentHp = stats.hp;
   }

   protected virtual void Update()
   {
      if (PauseController.IsPaused) return;
      if (PlayerCombatController.Instance == null) return;

      if (!startled)
      {
         Collider[] hits = Physics.OverlapSphere(transform.position, stats.startleRange);
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

   // El movimiento se llama desde ArmAttackNode
   public IEnumerator MoveEnemy()
   {
      Transform player = PlayerCombatController.Instance.transform;
      Vector3 dir = (player.position - transform.position).normalized;

      if (Physics.Raycast(transform.position, dir, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
         yield return MoveDirectly(player);
      else
         yield return MoveWithPathfinding(player);

      succeeded = true;
      isMoving = false;
   }

   private IEnumerator MoveDirectly(Transform player)
   {
      NavMeshAgent agent = GetComponent<NavMeshAgent>();
      agent.speed = stats.speed;
      agent.stoppingDistance = stopDistance;
      agent.updateRotation = false;
      agent.enabled = true;
      agent.Warp(transform.position);

      while (Vector3.Distance(transform.position, player.position) > stopDistance)
      {
         if (agent.isOnNavMesh)
            agent.SetDestination(player.position);
         LookAtPlayer();

         Vector3 dir = (new Vector3(player.position.x, transform.position.y, player.position.z) - transform.position).normalized;
         if (Physics.Raycast(transform.position, dir, out RaycastHit hitWall, Mathf.Infinity) && !hitWall.collider.CompareTag("Player"))
         {
            agent.enabled = false;
            yield return MoveWithPathfinding(player);
            yield break;
         }

         yield return null;
         if (PauseController.IsPaused)
         {
            agent.isStopped = true;
            yield return WaitWhilePaused();
            agent.isStopped = false;
         }
      }

      agent.enabled = false;
   }

   private IEnumerator MoveWithPathfinding(Transform player)
   {
      Seeker seeker = GetComponent<Seeker>();
      List<Vector3> pathNodes = null;
      bool pathReady = false;

      seeker.StartPath(transform.position, player.position, (Path p) =>
      {
         if (!p.error)
            pathNodes = p.vectorPath;
         pathReady = true;
      });

      yield return new WaitUntil(() => pathReady);

      if (pathNodes == null || pathNodes.Count == 0)
      {
         succeeded = false;
         isMoving = false;
         yield break;
      }

      int nodeIndex = 0;
      while (Vector3.Distance(transform.position, player.position) > stopDistance)
      {
         if (nodeIndex >= pathNodes.Count)
         {
            pathReady = false;
            seeker.StartPath(transform.position, player.position, (Path p) =>
            {
               if (!p.error) pathNodes = p.vectorPath;
               pathReady = true;
            });
            yield return new WaitUntil(() => pathReady);
            nodeIndex = 0;
            if (pathNodes == null || pathNodes.Count == 0) yield break;
         }

         Vector3 target = pathNodes[nodeIndex];
         target.y = transform.position.y;

         if (Vector3.Distance(transform.position, target) < 0.5f)
         {
            nodeIndex++;
            continue;
         }

         Vector3 dirToPlayer = (player.position - transform.position).normalized;
         if (Physics.Raycast(transform.position, dirToPlayer, out RaycastHit hit, Mathf.Infinity) && hit.collider.CompareTag("Player"))
         {
            yield return MoveDirectly(player);
            yield break;
         }

         transform.position = Vector3.MoveTowards(transform.position, target, stats.speed * Time.deltaTime);
         transform.forward = (target - transform.position).normalized;
         yield return null;
         yield return WaitWhilePaused();
      }
   }

   // Se llama desde RangedAttackNode y SpawnMinionsNode
    public GameObject Spawn(GameObject prefab, Vector3 position)
    {
        if (prefab != null)
        {
            GameObject go = Instantiate(prefab, position, Quaternion.identity);
            if (go.TryGetComponent<Bullet>(out Bullet b))
            {
                if (this is Boss1Controller boss)
                    b.Initialize(gameObject, boss.boss3BulletsAttackData, boss.stats.attack);
            }
            return go;
        }
        return null;
    }
}