using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(CinemachineCamera))]
[RequireComponent(typeof(CinemachineFollow))]
public class MainCamera : MonoBehaviour
{
    Transform target;
    Transform cam;
    // Offset de vista isométrica
    [SerializeField] Vector3 offset = new Vector3(-10f, 10f, -10f);

    public static Vector3 isoForward { get; private set; }
    public static Vector3 isoRight { get; private set; }

    void Awake()
    {
        target = GameObject.FindWithTag("Player").transform;
        cam = GetComponent<Transform>();

        var cinemachineTarget = GetComponent<CinemachineCamera>().Target;
        cinemachineTarget.TrackingTarget = target;
        GetComponent<CinemachineCamera>().Target = cinemachineTarget;

        GetComponent<CinemachineFollow>().FollowOffset = offset;

        // Apuntar la cámara hacia el jugador desde el offset
        cam.rotation = Quaternion.LookRotation(-offset);

        // Calcular ejes de movimiento isométrico desde el offset configurado.
        // Usamos el offset, no la posición en runtime, para evitar bucles de
        // retroalimentación cuando CinemachineFollow orbita con la rotación del personaje.
        Vector3 flat = new Vector3(-offset.x, 0f, -offset.z);
        isoForward = flat.normalized;
        isoRight = Vector3.Cross(Vector3.up, isoForward);
    }
}
