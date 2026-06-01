using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "New Dialogue",
    menuName = "Dialogue System/Dialogue")]
public class DialogueData : ScriptableObject
{
    public List<DialogueNode> nodes = new();
}