using System;
using UnityEngine;

[Serializable]
public class DialogueNode
{
    public DialogueNodeType nodeType;

    public DialogueLine dialogueLine;
    public ConditionNode condition;
    public ChoiceNode choice;
    public FlagActionNode flagAction;
}

public enum DialogueNodeType
{
    Dialogue,
    Condition,
    Choice,
    FlagAction
}