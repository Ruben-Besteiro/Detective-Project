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
        DrawBox(col.bounds, color, duration);
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
