using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(DialogueNode))]
public class DialogueNodeDrawer : PropertyDrawer
{
    public override void OnGUI(
        Rect position,
        SerializedProperty property,
        GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var nodeType = property.FindPropertyRelative("nodeType");

        Rect rect = position;
        rect.height = EditorGUIUtility.singleLineHeight;

        EditorGUI.PropertyField(rect, nodeType);

        rect.y += EditorGUIUtility.singleLineHeight + 2;

        SerializedProperty dataProperty = null;

        switch ((DialogueNodeType)nodeType.enumValueIndex)
        {
            case DialogueNodeType.Dialogue:
                dataProperty = property.FindPropertyRelative("dialogueLine");
                break;

            case DialogueNodeType.Condition:
                dataProperty = property.FindPropertyRelative("condition");
                break;

            case DialogueNodeType.Choice:
                dataProperty = property.FindPropertyRelative("choice");
                break;

            case DialogueNodeType.FlagAction:
                dataProperty = property.FindPropertyRelative("flagAction");
                break;
        }

        if (dataProperty != null)
        {
            EditorGUI.PropertyField(
                rect,
                dataProperty,
                true);
        }

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(
        SerializedProperty property,
        GUIContent label)
    {
        float height = EditorGUIUtility.singleLineHeight + 4;

        var nodeType = property.FindPropertyRelative("nodeType");

        SerializedProperty dataProperty = null;

        switch ((DialogueNodeType)nodeType.enumValueIndex)
        {
            case DialogueNodeType.Dialogue:
                dataProperty = property.FindPropertyRelative("dialogueLine");
                break;

            case DialogueNodeType.Condition:
                dataProperty = property.FindPropertyRelative("condition");
                break;

            case DialogueNodeType.Choice:
                dataProperty = property.FindPropertyRelative("choice");
                break;

            case DialogueNodeType.FlagAction:
                dataProperty = property.FindPropertyRelative("flagAction");
                break;
        }

        if (dataProperty != null)
        {
            height += EditorGUI.GetPropertyHeight(
                dataProperty,
                true);
        }

        return height;
    }
}