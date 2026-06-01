using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance { get; private set; }

    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private Image dialogueBox;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private TMP_Text speakerNameText;

    [Header("Character Portraits")]
    [SerializeField] private Image leftPortrait;
    [SerializeField] private Image rightPortrait;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ShowDialogueUI()
    {
        dialoguePanel.SetActive(true);
    }

    public void HideDialogueUI()
    {
        dialoguePanel.SetActive(false);
    }

    public void SetDialogueText(string text)
    {
        dialogueText.text = text;
    }

    public void SetSpeakerName(string speakerName)
    {
        speakerNameText.text = speakerName;
    }

    public void SetLeftPortrait(Sprite portrait)
    {
        leftPortrait.sprite = portrait;
        leftPortrait.enabled = portrait != null;
    }

    public void SetRightPortrait(Sprite portrait)
    {
        rightPortrait.sprite = portrait;
        rightPortrait.enabled = portrait != null;
    }
}