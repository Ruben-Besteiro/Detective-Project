using UnityEngine;

public class ShootState : State
{
    float timer;

    public ShootState(PlayerCombatController controller) : base(controller)
    {
        Debug.Log("Has entrado en estado SHOOT");
    }

    public override void Enter()
    {
        timer = controller.currentGunData.cooldown;
        controller.RotateTowardCursor();
        
        Vector3 spawnPos = controller.transform.position + controller.transform.forward + Vector3.up * 0.5f;
        GameObject bullet = Object.Instantiate(controller.currentGunData.bulletPrefab, spawnPos, controller.transform.rotation);
        bullet.GetComponent<Bullet>().Initialize(controller.gameObject, controller.currentGunData.bulletSpeed, controller.currentGunData.bulletLifetime);
    }

    public override void Update()
    {
        // Se puede cancelar el cooldown del disparo con otro disparo
        timer -= Time.deltaTime;
        if (timer <= 0f)
            controller.ChangeState(controller.moveInput.sqrMagnitude > 0.01f
                ? (State)new MoveState(controller)
                : new IdleState(controller));
    }
}