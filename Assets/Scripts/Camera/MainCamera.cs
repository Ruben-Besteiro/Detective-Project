using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineFollow))]
public class MainCamera : MonoBehaviour
{
    [SerializeField] Transform target;
    // Offset de vista isométrica clásica
    [SerializeField] Vector3 offset = new Vector3(-10f, 10f, -10f);

    void Awake()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player").transform;

        var cinemachineTarget = GetComponent<CinemachineCamera>().Target;
        cinemachineTarget.TrackingTarget = target;
        GetComponent<CinemachineCamera>().Target = cinemachineTarget;

        GetComponent<CinemachineFollow>().FollowOffset = offset;

        // Apuntar la cámara hacia el jugador desde el offset
        transform.rotation = Quaternion.LookRotation(-offset);
    }
}
