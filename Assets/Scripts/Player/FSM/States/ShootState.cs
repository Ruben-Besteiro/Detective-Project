using UnityEngine;

public class ShootState : State
{
    float timer;
    Vector3 aimPoint;
    bool hasAimPoint;

    public ShootState(PlayerCombatController controller) : base(controller) {}

    public override void Enter()
    {
        timer = controller.currentGunData.cooldown;
        if (controller.HasAimPoint) aimPoint = controller.GetAimPoint();
        controller.RotateTowardCursor();

        Vector3 spawnPos = controller.transform.position + controller.transform.forward + Vector3.up * 0.5f;
        GameObject bullet = Object.Instantiate(controller.bulletPrefab, spawnPos, controller.transform.rotation);
        bullet.GetComponent<Bullet>().Initialize(controller.gameObject, controller.currentGunData, controller.stats.attack, controller.damageMultiplier);    }

    public override void Update()
    {
        controller.Move();
        if (hasAimPoint)
            controller.RotateTowardCursor();

        timer -= Time.deltaTime;
        if (timer <= 0f)
            controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
    }
}