using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChoiceManager : MonoBehaviour
{
    public static ChoiceManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panel;
    [SerializeField] private RectTransform panelTransform;

    [Header("Choices")]
    [SerializeField] private Button buttonPrefab;
    [SerializeField] private Transform buttonContainer;

    private readonly List<Button> activeButtons = new();

    private int selectedOption = -1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IEnumerator ShowChoices(List<ChoiceOption> options, System.Action<int> onSelected)
    {
        selectedOption = -1;

        ClearButtons();

        panel.SetActive(true);

        panelTransform.localScale = Vector3.zero;

        yield return panelTransform
            .DOScale(1f, 0.25f)
            .SetEase(Ease.OutBack)
            .WaitForCompletion();

        for (int i = 0; i < options.Count; i++)
        {
            CreateButton(options[i], i);
        }

        while (selectedOption < 0)
        {
            yield return null;
        }

        yield return panelTransform
            .DOScale(0f, 0.2f)
            .SetEase(Ease.InBack)
            .WaitForCompletion();

        panel.SetActive(false);

        onSelected?.Invoke(selectedOption);
    }

    private void CreateButton(ChoiceOption option, int index)
    {
        Button button =
            Instantiate(buttonPrefab, buttonContainer);

        activeButtons.Add(button);

        TMP_Text text =
            button.GetComponentInChildren<TMP_Text>();

        text.text = option.optionText;

        button.onClick.AddListener(() =>
        {
            selectedOption = index;
        });
    }
    private void ClearButtons()
    {
        foreach (Button button in activeButtons)
        {
            Destroy(button.gameObject);
        }

        activeButtons.Clear();
    }
}