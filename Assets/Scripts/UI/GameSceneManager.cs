using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections;

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    [Header("UI")]
    [SerializeField] private Image fadePanel;
    [SerializeField] private RectTransform loadingIcon;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 0.5f;

    private string mainScene;
    private string secondaryScene;
    private Tween loadingTween;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        fadePanel.gameObject.SetActive(false);
        fadePanel.color = new Color(0, 0, 0, 0);
        loadingIcon.gameObject.SetActive(false);
    }

    /* SCENE LOAD */

    public void LoadScene(string sceneName, SceneTransition transition, bool showLoadingIcon)
    {
        //SoundManager.Instance.StopMusic();
        
        if (transition == SceneTransition.Instant)
        {
            SceneManager.LoadScene(sceneName);
            mainScene = sceneName;
            return;
        }

        StartCoroutine(LoadSceneFade(sceneName, showLoadingIcon));
    }

    public IEnumerator LoadSceneFade(string sceneName, bool showLoadingIcon)
    {
        yield return FadeIn();

        if (showLoadingIcon)
            StartLoadingIcon();

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName);
        op.allowSceneActivation = false;

        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;
        yield return null;

        if (showLoadingIcon)
            StopLoadingIcon();

        mainScene = sceneName;
        yield return FadeOut();
    }

    /* SECONDARY SCENE */

    public void LoadSecondaryScene(string sceneName, SceneTransition transition, bool showLoadingIcon)
    {
        if (!string.IsNullOrEmpty(secondaryScene))
            return;

        StartCoroutine(LoadSecondary(sceneName, transition, showLoadingIcon));
    }

    private IEnumerator LoadSecondary(string sceneName, SceneTransition transition, bool showLoadingIcon)
    {
        if (transition == SceneTransition.FadeBlack)
        {
            yield return FadeIn();
            if (showLoadingIcon)
                StartLoadingIcon();
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        while (!op.isDone)
            yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(sceneName));
        secondaryScene = sceneName;

        if (transition == SceneTransition.FadeBlack)
        {
            if (showLoadingIcon)
                StopLoadingIcon();

            yield return FadeOut();
        }
    }

    public void UnloadSecondaryScene(SceneTransition transition, bool showLoadingIcon)
    {
        if (string.IsNullOrEmpty(secondaryScene))
            return;

        StartCoroutine(UnloadSecondary(transition, showLoadingIcon));
    }

    private IEnumerator UnloadSecondary(SceneTransition transition, bool showLoadingIcon)
    {
        if (transition == SceneTransition.FadeBlack)
        {
            yield return FadeIn();
            if (showLoadingIcon)
                StartLoadingIcon();
        }

        AsyncOperation op = SceneManager.UnloadSceneAsync(secondaryScene);
        while (!op.isDone)
            yield return null;

        SceneManager.SetActiveScene(SceneManager.GetSceneByName(mainScene));
        secondaryScene = null;

        if (transition == SceneTransition.FadeBlack)
        {
            if (showLoadingIcon)
                StopLoadingIcon();

            yield return FadeOut();
        }
    }

    /* UI TWEENS */

    private IEnumerator FadeIn()
    {
        fadePanel.gameObject.SetActive(true);
        yield return fadePanel.DOFade(1f, fadeDuration).WaitForCompletion();
    }

    private IEnumerator FadeOut()
    {
        yield return fadePanel.DOFade(0f, fadeDuration).WaitForCompletion();
        fadePanel.gameObject.SetActive(false);
    }

    private void StartLoadingIcon()
    {
        loadingIcon.gameObject.SetActive(true);
        loadingTween = loadingIcon
            .DORotate(new Vector3(0, 0, -360), 1f, RotateMode.FastBeyond360)
            .SetLoops(-1)
            .SetEase(Ease.Linear);
    }

    private void StopLoadingIcon()
    {
        loadingTween?.Kill();
        loadingIcon.gameObject.SetActive(false);
    }
}

public enum SceneTransition
{
    Instant,
    FadeBlack
}

