using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FeedbackAnimationManager : MonoBehaviour
{
    public static FeedbackAnimationManager Instance;
    [SerializeField] private GameObject feedbackRoot;
    [SerializeField] private Image image;
    [SerializeField] private TMP_Text text;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        feedbackRoot.SetActive(false);
    }

    public void PlayPickupAnimation(Sprite icon, string itemName)
    {
        StartCoroutine(PickupRoutine(icon, itemName));
    }

    private IEnumerator PickupRoutine(Sprite icon, string itemName)
    {
        PauseController.PauseForFeedback();

        feedbackRoot.SetActive(true);

        image.sprite = icon;
        text.text = itemName;

        transform.localScale = Vector3.zero;

        Sequence seq = DOTween.Sequence();

        seq.Append(image.transform.DOScale(1.2f, 0.25f));
        seq.Join(image.transform.DOMoveY(image.transform.position.y + 100f, 0.4f));
        seq.Join(image.transform.DOShakeRotation(0.4f, 10f));
        seq.AppendInterval(0.4f);
        seq.Append(image.transform.DOScale(0f, 0.25f));

        yield return seq.WaitForCompletion();

        feedbackRoot.SetActive(false);

        PauseController.ResumeFeedback();
    }
}