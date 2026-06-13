using UnityEngine;

public class ShootState : State
{
    float timer;
    const float cooldown = 1f;

    float projectileSpeed = 10;
    float projectileLifetime = 2;

    public ShootState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado SHOOT");
    }

    public override void Enter()
    {
        timer = cooldown;
        controller.RotateTowardCursor();
        
        Vector3 spawnPos = controller.transform.position + controller.transform.forward + Vector3.up * 0.5f;
        GameObject bullet = Object.Instantiate(controller.bulletPrefab, spawnPos, controller.transform.rotation);
        bullet.GetComponent<Bullet>().Initialize(controller.gameObject, projectileSpeed, projectileLifetime);
    }

    public override void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
            controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
    }
}