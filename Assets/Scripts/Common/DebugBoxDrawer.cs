using UnityEngine;

public class DebugBoxDrawer : MonoBehaviour
{
    private float lifetime;
    private float timer;

    private Vector3 startScale;
    [SerializeField] private MeshRenderer meshRenderer;

    public static void DrawBox(Vector3 center, Vector3 size, Quaternion rotation, Color color, float duration = 1f)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "DEBUG_BOX";

        Object.Destroy(cube.GetComponent<Collider>());

        cube.transform.position = center;
        cube.transform.rotation = rotation;
        cube.transform.localScale = size;

        var drawer = cube.AddComponent<DebugBoxDrawer>();
        drawer.Initialize(color, duration);
    }

    public static void DrawBox(Bounds bounds, Color color, float duration = 1f)
    {
        DrawBox(bounds.center, bounds.size, Quaternion.identity, color, duration);
    }

    public static void DrawBox(Collider col, Color color, float duration = 1f)
    {
        Bounds local = GetLocalBounds(col);
        Vector3 worldCenter = col.transform.TransformPoint(local.center);
        Vector3 worldSize = Vector3.Scale(local.size, col.transform.lossyScale);
        worldSize = new Vector3(Mathf.Abs(worldSize.x), Mathf.Abs(worldSize.y), Mathf.Abs(worldSize.z));
        DrawBox(worldCenter, worldSize, col.transform.rotation, color, duration);
    }

    private static Bounds GetLocalBounds(Collider col)
    {
        if (col is BoxCollider box)
            return new Bounds(box.center, box.size);
        if (col is SphereCollider sphere)
            return new Bounds(sphere.center, Vector3.one * sphere.radius * 2f);
        if (col is CapsuleCollider capsule)
        {
            float r = capsule.radius * 2f;
            float h = Mathf.Max(capsule.height, r);
            Vector3 size = capsule.direction == 0 ? new Vector3(h, r, r)
                         : capsule.direction == 2 ? new Vector3(r, r, h)
                         : new Vector3(r, h, r);
            return new Bounds(capsule.center, size);
        }
        return new Bounds(Vector3.zero, Vector3.one);
    }

    private void Initialize(Color color, float duration)
    {
        lifetime = duration;
        timer = duration;
        startScale = transform.localScale;

        meshRenderer = GetComponent<MeshRenderer>();
        var mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        mat.SetFloat("_Surface", 1f);
        mat.SetFloat("_Blend", 0f);
        mat.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetFloat("_ZWrite", 0f);
        mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        mat.color = color;
        meshRenderer.material = mat;
    }

    private void Update()
    {
        timer -= Time.deltaTime;

        float t = Mathf.Clamp01(timer / lifetime);
        transform.localScale = startScale * t;

        Color c = meshRenderer.material.color;
        c.a = t;
        meshRenderer.material.color = c;

        if (timer <= 0f)
            Destroy(gameObject);
    }
}
