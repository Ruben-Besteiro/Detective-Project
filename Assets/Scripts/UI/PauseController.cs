using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public class PauseController : MonoBehaviour
{
    public static PauseController Instance { get; private set; }

    public static event Action OnPauseStarted;
    public static event Action OnPauseEnded;

    [Header("UI")]
    [SerializeField] private GameObject overlayPanel;

    [SerializeField] private RectTransform pauseMenu;
                     private Vector2 pauseMenuOGPos;

    [Header("Inventory")]
    [SerializeField] private RectTransform inventoryPanel;
    private Vector2 inventoryPanelOGPos;

    [Header("Hypotesis")]
    [SerializeField] private HypothesisSet currentSet;
    [SerializeField] private RectTransform hypothesisPanel;
                     private Vector2 hypotesisMenuOGPos;
    [SerializeField] private HypothesisCardUI card1;
    [SerializeField] private HypothesisCardUI card2;
    [SerializeField] private HypothesisCardUI card3;

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;

    private PauseReason pauseReason = PauseReason.None;

    public static bool IsPaused =>
    Instance != null &&
    Instance.pauseReason != PauseReason.None;

    public static bool IsMenuPaused =>
        Instance != null &&
        Instance.pauseReason == PauseReason.Menu;

    public static bool IsFeedbackPaused =>
        Instance != null &&
        Instance.pauseReason == PauseReason.FeedbackAnimation;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        pauseMenuOGPos = pauseMenu.anchoredPosition;
        inventoryPanelOGPos = inventoryPanel.anchoredPosition;
        hypotesisMenuOGPos = hypothesisPanel.anchoredPosition;
    }

    private void Start()
    {
        overlayPanel.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        inventoryPanel.gameObject.SetActive(false);
        hypothesisPanel.gameObject.SetActive(false);
    }

    public static void Pause()
    {
        if (Instance == null || Instance.pauseReason != PauseReason.None)
            return;

        Instance.StartPause();
    }

    public static void Unpause()
    {
        if (Instance == null)
            return;

        if (Instance.pauseReason != PauseReason.Menu)
            return;

        Instance.StartCoroutine(
            Instance.UnpauseRoutine()
        );
    }

    private void StartPause()
    {
        pauseReason = PauseReason.Menu;
        OnPauseStarted?.Invoke();

        card1.Setup(currentSet.first);
        card2.Setup(currentSet.second);
        card3.Setup(currentSet.third);

        overlayPanel.SetActive(true);
        pauseMenu.gameObject.SetActive(true);
        hypothesisPanel.gameObject.SetActive(true);
        inventoryPanel.gameObject.SetActive(true);

        Vector2 startPosition = new Vector2(-Screen.width, pauseMenu.anchoredPosition.y);
        Vector2 inventoryStartPos = new Vector2(Screen.width, inventoryPanelOGPos.y);
        Vector2 hypotesisStartPos = new Vector2(Screen.width, inventoryPanelOGPos.y);

        pauseMenu.anchoredPosition = startPosition;
        inventoryPanel.anchoredPosition = inventoryStartPos;

        pauseMenu.DOAnchorPos(pauseMenuOGPos, animationDuration).SetEase(Ease.OutBack);
        inventoryPanel.DOAnchorPos(inventoryPanelOGPos, animationDuration).SetEase(Ease.OutBack);
        hypothesisPanel.DOAnchorPos(hypotesisMenuOGPos, animationDuration).SetEase(Ease.OutBack);
    }

    private IEnumerator UnpauseRoutine()
    {
        pauseMenu.DOAnchorPosX(-pauseMenu.rect.width, animationDuration).SetEase(Ease.InBack).SetUpdate(true);
        inventoryPanel.DOAnchorPosX(Screen.width, animationDuration).SetEase(Ease.InBack).SetUpdate(true);

        yield return new WaitForSecondsRealtime(animationDuration);

        overlayPanel.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
        inventoryPanel.gameObject.SetActive(false);
        hypothesisPanel.gameObject.SetActive(false);

        pauseReason = PauseReason.None;

        OnPauseEnded?.Invoke();
    }

    public static void PauseForFeedback()
    {
        if (Instance == null)
            return;

        if (Instance.pauseReason != PauseReason.None)
            return;

        Instance.pauseReason =
            PauseReason.FeedbackAnimation;

        OnPauseStarted?.Invoke();
    }
    public static void ResumeFeedback()
    {
        if (Instance == null)
            return;

        if (Instance.pauseReason != PauseReason.FeedbackAnimation)
            return;

        Instance.pauseReason =
            PauseReason.None;

        OnPauseEnded?.Invoke();
    }
}

public enum PauseReason
{
    None,
    Menu,
    FeedbackAnimation
}