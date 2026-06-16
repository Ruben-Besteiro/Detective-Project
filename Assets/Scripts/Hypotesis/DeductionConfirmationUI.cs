using UnityEngine;

public class DeductionConfirmationUI : MonoBehaviour
{
    public static DeductionConfirmationUI Instance;

    private DeductionCard currentCard;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void Show(DeductionCard card)
    {
        currentCard = card;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void Confirm()
    {
        if (currentCard == null)
            return;

        GameDataManager.Instance.currentHypothesis =
            currentCard.Hypothesis;

        PauseController.Unpause();

        GameSceneManager.Instance.LoadScene(
            currentCard.BattleScene,
            SceneTransition.FadeBlack,
            true);

        Hide();
    }

    public void Cancel()
    {
        Hide();
    }
}