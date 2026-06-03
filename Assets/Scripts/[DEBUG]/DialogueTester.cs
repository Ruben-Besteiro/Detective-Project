using UnityEngine;

public class DialogueTester : MonoBehaviour
{
    [SerializeField] private DialogueData dialogue;

    public void StartTestDialogue()
    {
        DialogueManager.StartDialogue(dialogue);
    }
}