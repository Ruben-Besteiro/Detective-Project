using TMPro;
using UnityEngine;

public class HypothesisCardUI : MonoBehaviour
{
    [SerializeField] private TMP_Text title;
    [SerializeField] private TMP_Text description;

    public void Setup(HypothesisData data)
    {
        var version = data.GetCurrentVersion();

        title.text = version.title;
        description.text = version.description;
    }
}