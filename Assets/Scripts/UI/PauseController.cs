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

    [Header("Animation")]
    [SerializeField] private float animationDuration = 0.3f;

    private bool isPaused;

    public static bool IsPaused =>
        Instance != null && Instance.isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        pauseMenuOGPos = pauseMenu.anchoredPosition;
    }

    private void Start()
    {
        overlayPanel.SetActive(false);
        pauseMenu.gameObject.SetActive(false);
    }

    public static void Pause()
    {
        if (Instance == null || Instance.isPaused)
            return;

        Instance.StartPause();
    }

    public static void Unpause()
    {
        if (Instance == null || !Instance.isPaused)
            return;

        Instance.StartCoroutine(Instance.UnpauseRoutine());
    }

    private void StartPause()
    {
        SoundManager.Instance.PauseMusic();
        SoundManager.Instance.PlaySFX(SFX.Pause);
        isPaused = true;
        OnPauseStarted?.Invoke();

        overlayPanel.SetActive(true);
        pauseMenu.gameObject.SetActive(true);

        Vector2 startPosition = new Vector2(-pauseMenu.rect.width, pauseMenu.anchoredPosition.y);

        pauseMenu.anchoredPosition = startPosition;

        pauseMenu.DOAnchorPos(pauseMenuOGPos, animationDuration).SetEase(Ease.OutBack);
    }

    private IEnumerator UnpauseRoutine()
    {
        SoundManager.Instance.PlaySFX(SFX.Yes);
        SoundManager.Instance.ResumeMusic();
        pauseMenu.DOAnchorPosX(-pauseMenu.rect.width, animationDuration).SetEase(Ease.InBack).SetUpdate(true);

        yield return new WaitForSecondsRealtime(animationDuration);

        overlayPanel.SetActive(false);
        pauseMenu.gameObject.SetActive(false);

        isPaused = false;

        OnPauseEnded?.Invoke();
    }
}