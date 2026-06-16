using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeductionCard : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("Deduction")]
    [SerializeField] private Hypotheses hypothesis;
    [SerializeField] private string battleScene;

    [Header("Float")]
    [SerializeField] private float amplitude = 10f;
    [SerializeField] private float speed = 2f;

    [Header("Pulse")]
    [SerializeField] private float pulseScale = 1.1f;
    [SerializeField] private float pulseDuration = 0.5f;

    private RectTransform rectTransform;
    private Vector2 startPosition;

    private Tween pulseTween;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        startPosition = rectTransform.anchoredPosition;
    }

    private void Update()
    {
        float y =
            Mathf.Sin(Time.unscaledTime * speed)
            * amplitude;

        rectTransform.anchoredPosition =
            startPosition + Vector2.up * y;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        pulseTween?.Kill();

        pulseTween = transform
            .DOScale(pulseScale, pulseDuration)
            .SetLoops(-1, LoopType.Yoyo)
            .SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        pulseTween?.Kill();

        transform
            .DOScale(Vector3.one, 0.15f)
            .SetUpdate(true);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        DeductionConfirmationUI.Instance
            .Show(this);
    }

    public Hypotheses Hypothesis => hypothesis;

    public string BattleScene => battleScene;
}