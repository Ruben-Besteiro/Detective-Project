using System;
using System.Collections.Generic;
using UnityEngine;


[Serializable]
public class DialogueLine
{
    public string speakerName;

    [TextArea(3, 8)]
    public string text;

    public Sprite leftPortrait;
    public Sprite rightPortrait;
}

[Serializable]
public class ConditionNode
{
    public string flagName;
    public bool expectedValue = true;

    [Header("If True")]
    public bool trueContinueCurrent = true;
    public DialogueData trueNextDialogue;

    [Header("If False")]
    public bool falseContinueCurrent = true;
    public DialogueData falseNextDialogue;
}

[Serializable]
public class ChoiceOption
{
    public string optionText;

    public string flagToSet;

    [Header("Flow")]
    public bool continueCurrentDialogue = true;

    public DialogueData nextDialogue;
}

[Serializable]
public class ChoiceNode
{
    public List<ChoiceOption> options = new();
}

[Serializable]
public class FlagActionNode
{
    public string flagName;

    public bool newValue;
}

