using DG.Tweening;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject interactCanvas;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    [SerializeField] private float textSpeed = 0.02f;
    [SerializeField] private float portraitAnimDuration = 0.35f;

    public static DialogueManager Instance { get; private set; }

    public static event Action OnDialogueStarted;
    public static event Action OnDialogueFinished;

    private DialogueData currentDialogue;
    private int currentNodeIndex;
    private bool skipCurrentDialogue;

    private Coroutine runningDialogue;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!isWaitingForInput)
            {
                isSkippingText = true;
            }
        }
    }


    // ENTRY POINT (STATIC)
    public static void StartDialogue(DialogueData dialogue)
    {
        if (Instance == null)
            return;

        Instance.ResetPortraits();
        Instance.SwitchDialogue(dialogue);
    }

    private void ResetPortraits()
    {
        leftPortrait.gameObject.SetActive(false);
        rightPortrait.gameObject.SetActive(false);
    }

    private void SwitchDialogue(DialogueData newDialogue)
    {
        // Si ya hay uno corriendo, lo detenemos
        if (runningDialogue != null)
        {
            skipCurrentDialogue = true;
            StopCoroutine(runningDialogue);
        }

        currentDialogue = newDialogue;
        currentNodeIndex = 0;
        skipCurrentDialogue = false;

        runningDialogue = StartCoroutine(PlayDialogue());
    }

    // MAIN COROUTINE
    private IEnumerator PlayDialogue()
    {
        interactCanvas?.SetActive(false);
        OnDialogueStarted?.Invoke();
        GetComponent<Canvas>().enabled = true;
        Debug.Log($"[Dialogue] START: {currentDialogue.name}");

        currentNodeIndex = 0;

        while (currentDialogue != null &&
               currentNodeIndex < currentDialogue.nodes.Count)
        {
            var node = currentDialogue.nodes[currentNodeIndex];

            yield return ProcessNode(node);

            // Si se ha solicitado cambio de di�logo, salimos inmediatamente
            if (skipCurrentDialogue)
            {
                yield break;
            }

            currentNodeIndex++;
        }
        GetComponent<Canvas>().enabled = false;
        Debug.Log($"[Dialogue] END: {currentDialogue.name}");

        interactCanvas?.SetActive(true);
        OnDialogueFinished?.Invoke();
        runningDialogue = null;
    }

    // NODE PROCESSOR
    private IEnumerator ProcessNode(DialogueNode node)
    {
        switch (node.nodeType)
        {
            case DialogueNodeType.Dialogue:
                yield return ProcessDialogue(node);
                break;

            case DialogueNodeType.Condition:
                yield return ProcessCondition(node);
                break;

            case DialogueNodeType.Choice:
                yield return ProcessChoice(node);
                break;

            case DialogueNodeType.FlagAction:
                yield return ProcessFlagAction(node);
                break;
        }
    }

    // NODE TYPES
    private bool isSkippingText;
    private bool isWaitingForInput;
    private IEnumerator ProcessDialogue(DialogueNode node)
    {
        var line = node.dialogueLine;

        ResetPortraits();

        yield return HandlePortrait(leftPortrait, line.leftPortrait, true);
        yield return HandlePortrait(rightPortrait, line.rightPortrait, false);

        yield return UpdateSpeakerName(line.speakerName);

        yield return TypeText(line.text);

        yield return WaitForClick();
    }
    private IEnumerator HandlePortrait(Image img, Sprite newSprite, bool isLeft)
    {
        // CASO 1: no hay sprite -> ocultar si estaba activo
        if (newSprite == null)
        {
            if (img.gameObject.activeSelf)
            {
                yield return img.transform
                    .DOScale(0f, portraitAnimDuration)
                    .SetEase(Ease.InBack)
                    .WaitForCompletion();

                img.gameObject.SetActive(false);
            }

            yield break;
        }

        // CASO 2: aparece por primera vez
        if (!img.gameObject.activeSelf)
        {
            img.sprite = newSprite;
            img.transform.localScale = Vector3.zero;

            img.gameObject.SetActive(true);

            yield return img.transform
                .DOScale(1f, portraitAnimDuration)
                .SetEase(Ease.OutBack)
                .WaitForCompletion();

            yield break;
        }

        // CASO 3: mismo sprite -> no hacer nada
        if (img.sprite == newSprite)
            yield break;

        // CASO 4: cambio de sprite -> pulso r�pido
        yield return img.transform
            .DOPunchScale(Vector3.one * 0.15f, 0.2f)
            .WaitForCompletion();

        img.sprite = newSprite;
    }
    private IEnumerator UpdateSpeakerName(string name)
    {
        speakerNameText.text = name;
        yield return null;
    }
    private IEnumerator TypeText(string text)
    {
        dialogueText.text = "";
        isSkippingText = false;

        for (int i = 0; i < text.Length; i++)
        {
            if (isSkippingText)
            {
                dialogueText.text = text;
                yield break;
            }

            dialogueText.text += text[i];
            yield return new WaitForSeconds(textSpeed);
        }
    }
    private IEnumerator WaitForClick()
    {
        isWaitingForInput = true;

        while (!Input.GetMouseButtonDown(0) &&
               !Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        isWaitingForInput = false;
    }

    private IEnumerator ProcessCondition(DialogueNode node)
    {
        Debug.Log("[Condition] Evaluating condition node");
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ProcessChoice(DialogueNode node)
    {
        Debug.Log("[Choice] Presenting options");
        yield return new WaitForSeconds(1f);
    }

    private IEnumerator ProcessFlagAction(DialogueNode node)
    {
        Debug.Log($"[Flag] {node.flagAction.flagName}");

        yield return new WaitForSeconds(1f);

    }
}