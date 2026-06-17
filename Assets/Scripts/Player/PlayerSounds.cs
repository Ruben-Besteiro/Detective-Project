using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class PlayerSounds : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerController player;

    [Header("Footsteps")]
    [SerializeField] private EventReference footstepEvent;

    [SerializeField] private float minStepInterval = 0.35f;
    [SerializeField] private float maxStepInterval = 0.6f;

    private float stepTimer;

    private void Update()
    {
        float movement = player.movement;

        if (movement <= 0.05f)
        {
            stepTimer = maxStepInterval;
            return;
        }

        float interval = Mathf.Lerp(maxStepInterval, minStepInterval, movement);

        stepTimer += Time.deltaTime;

        if (stepTimer >= interval)
        {
            stepTimer = 0f;

            PlayFootstep();
        }
    }

    private void PlayFootstep()
    {
        EventInstance instance = RuntimeManager.CreateInstance(footstepEvent);

        switch (GetSurface())
        {
            case SurfaceType.Grass:
                instance.setParameterByNameWithLabel( "Footsteps", "Grass");
                break;

            default:
                instance.setParameterByNameWithLabel("Footsteps", "Stone");
                break;
        }

        RuntimeManager.AttachInstanceToGameObject(
            instance,
            transform);

        instance.start();
        instance.release();
    }

    private SurfaceType GetSurface()
    {
        Ray ray = new Ray(
            transform.position + Vector3.up * 0.1f,
            Vector3.down);

        if (Physics.Raycast( ray, out RaycastHit hit, 2f))
        {
            bool surface = hit.collider.CompareTag("Ground_Grass");

            if (surface)
            {  return SurfaceType.Grass; }
        }

        return SurfaceType.Stone;
    }
}

public enum SurfaceType
{
    Stone,
    Grass
}