using UnityEngine;

public class FloatingCard : MonoBehaviour
{
    [SerializeField] float amplitude = 10f;
    [SerializeField] float speed = 2f;
    [SerializeField] float offset = 2f;

    Vector2 startPos;

    void Awake()
    {
        startPos = ((RectTransform)transform).anchoredPosition;
    }

    void Update()
    {
        float y = Mathf.Sin((Time.unscaledTime + offset) * speed) * amplitude;
        ((RectTransform)transform).anchoredPosition = startPos + Vector2.up * y;
    }
}