using DG.Tweening;
using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using FMODUnity;

public class DialogueManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text speakerNameText;
    [SerializeField] private TMP_Text dialogueText;

    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    [SerializeField] private float textSpeed = 0.02f;
    [SerializeField] private float portraitAnimDuration = 0.35f;

    [Header("Audio")]
    [SerializeField] private EventReference letterSound;
    [SerializeField] private EventReference lineFinishedSound;

    public static DialogueManager Instance { get; private set; }

    public static event Action OnDialogueStarted;
    public static event Action OnDialogueFinished;

    private DialogueData currentDialogue;
    private DialogueData pendingDialogue;
    private int currentNodeIndex;

    private Coroutine runningDialogue;
    private InputAction advanceDialogueAction;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        advanceDialogueAction = new InputAction(
            "AdvanceDialogue",
            InputActionType.Button);

        advanceDialogueAction.AddBinding("<Mouse>/leftButton");
        advanceDialogueAction.AddBinding("<Keyboard>/space");
    }

    private void PlayLetterSound()
    { SoundManager.Instance.PlayOneShot(letterSound); }

    private void PlayLineFinishedSound()
    { SoundManager.Instance.PlayOneShot(lineFinishedSound); }

    private void OnEnable()
    { advanceDialogueAction?.Enable(); }
    private void OnDisable()
    { advanceDialogueAction?.Disable(); }
    private bool AdvancePressed()
    { return advanceDialogueAction.WasPressedThisFrame(); }

    private void Update()
    {
        if (AdvancePressed())
        {
            if (isWaitingForInput)
            {
                advanceRequested = true;
            }
            else
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
        dialoguePanel.SetActive(true);
        // Si ya hay uno corriendo, lo detenemos
        if (runningDialogue != null)
        {
            StopCoroutine(runningDialogue);
        }

        currentDialogue = newDialogue;
        currentNodeIndex = 0;

        runningDialogue = StartCoroutine(PlayDialogue());
    }

    // MAIN COROUTINE
    private IEnumerator PlayDialogue()
    {
        OnDialogueStarted?.Invoke();
        GetComponent<Canvas>().enabled = true;

        currentNodeIndex = 0;

        while (currentDialogue != null && currentNodeIndex < currentDialogue.nodes.Count)
        {
            var node = currentDialogue.nodes[currentNodeIndex];

            yield return ProcessNode(node);

            if (pendingDialogue != null)
            {
                currentDialogue = pendingDialogue;
                pendingDialogue = null;

                currentNodeIndex = 0;

                continue;
            }

            currentNodeIndex++;
        }

        GetComponent<Canvas>().enabled = false;
        dialoguePanel.SetActive(false);
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
    private bool advanceRequested;
    private IEnumerator ProcessDialogue(DialogueNode node)
    {
        var line = node.dialogueLine;

        ResetPortraits();
        dialogueText.text = "";
        yield return UpdateSpeakerName(line.speakerName);

        StartCoroutine(HandlePortrait(leftPortrait, line.leftPortrait, true));
        yield return HandlePortrait(rightPortrait, line.rightPortrait, false);

        yield return TypeText(line.text);

        yield return WaitForClick();
    }
    private IEnumerator HandlePortrait(Image img, Sprite newSprite, bool isLeft) 
    { 
        if (newSprite == null) 
        {
            if (img.gameObject.activeSelf)
            { 
                yield return img.transform.DOScale(0f, portraitAnimDuration).SetEase(Ease.InBack).WaitForCompletion();
                img.gameObject.SetActive(false); 
            } 
            yield break; 
        }

        img.gameObject.SetActive(true);

        if (img.sprite == null) 
        { 
            img.sprite = newSprite; 
            img.transform.localScale = Vector3.zero; 
            yield return img.transform.DOScale(1f, portraitAnimDuration).SetEase(Ease.OutBack).WaitForCompletion(); 
            yield break; 
        }

        if (img.sprite != newSprite)
        { yield return img.transform.DOPunchScale(Vector3.one * 0.15f, 0.2f).WaitForCompletion(); }
            
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

                PlayLineFinishedSound();

                yield break;
            }

            dialogueText.text += text[i];

            if (!char.IsWhiteSpace(text[i]))
            { PlayLetterSound(); }

            yield return new WaitForSeconds(textSpeed);
        }

        PlayLineFinishedSound();
    }

    private IEnumerator WaitForClick()
    {
        isWaitingForInput = true;
        advanceRequested = false;

        while (!advanceRequested)
        {
            yield return null;
        }

        isWaitingForInput = false;
    }

    private IEnumerator ProcessCondition(DialogueNode node)
    {
        var condition = node.condition;

        bool result =
            FlagManager.Instance.HasFlag(condition.flagName)
            == condition.expectedValue;

        Debug.Log(
            $"[Condition] {condition.flagName} == {condition.expectedValue} => {result}");

        if (result)
        {
            if (!condition.trueContinueCurrent)
            {
                RequestDialogueSwitch(
                    condition.trueNextDialogue);
            }
        }
        else
        {
            if (!condition.falseContinueCurrent)
            {
                RequestDialogueSwitch(
                    condition.falseNextDialogue);
            }
        }

        yield return null;
    }
    private void RequestDialogueSwitch(DialogueData dialogue)
    {
        pendingDialogue = dialogue;
    }

    private IEnumerator ProcessChoice(DialogueNode node)
    {
        ChoiceNode choiceNode = node.choice;

        int selectedIndex = -1;

        yield return StartCoroutine(ChoiceManager.Instance.ShowChoices(choiceNode.options, result => selectedIndex = result));
        ChoiceOption selectedOption = choiceNode.options[selectedIndex];
        if (!selectedOption.continueCurrentDialogue)
        {
            RequestDialogueSwitch(
                selectedOption.nextDialogue);
        }
    }

    private IEnumerator ProcessFlagAction(DialogueNode node)
    {
        FlagManager.Instance.SetFlag(
            node.flagAction.flagName,
            node.flagAction.newValue);

        yield return null;
    }
}

