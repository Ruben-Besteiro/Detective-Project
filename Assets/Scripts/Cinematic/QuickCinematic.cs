using System.Collections;
using UnityEngine;

public class QuickCinematic : MonoBehaviour
{
    [Header("Dialogue")]
    [SerializeField] private DialogueData dialogue;

    [Header("Timing")]
    [SerializeField] private float startDelay = 0f;
    [SerializeField] private float endDelay = 0f;

    [Header("Next Scene")]
    [SerializeField] private string nextScene;
    [SerializeField] private SceneTransition transition = SceneTransition.FadeBlack;
    [SerializeField] private bool showLoadingIcon = true;

    private void Start()
    {
        DialogueManager.OnDialogueFinished += HandleDialogueEnded;

        StartCoroutine(StartDialogueRoutine());
    }

    private IEnumerator StartDialogueRoutine()
    {
        if (startDelay > 0)
            yield return new WaitForSeconds(startDelay);

        DialogueManager.StartDialogue(dialogue);
    }

    private void HandleDialogueEnded()
    {
        StartCoroutine(LoadNextSceneRoutine());
    }

    private IEnumerator LoadNextSceneRoutine()
    {
        DialogueManager.OnDialogueFinished -= HandleDialogueEnded;

        if (endDelay > 0)
            yield return new WaitForSeconds(endDelay);

        GameSceneManager.Instance.LoadScene(
            nextScene,
            transition,
            showLoadingIcon);
    }

    private void OnDestroy()
    {
        DialogueManager.OnDialogueFinished -= HandleDialogueEnded;
    }
}