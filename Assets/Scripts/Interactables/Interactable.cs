using System;
using TMPro;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    public static event Action<Interactable> OnEnterRange;
    public static event Action<Interactable> OnExitRange;

    [SerializeField] protected string displayName;
    [SerializeField] protected GameObject interactCanvas;
    protected TMP_Text promptText;

    protected virtual void Start()
    {
        promptText = interactCanvas.GetComponentInChildren<TMP_Text>();
    }

    public virtual void OnInteract() { }

    public void SetPromptActive(bool active)
    {
        if (interactCanvas != null)
            promptText.text = active ? promptText.text : "";
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnEnterRange?.Invoke(this);     // Añade el objeto a la lista de objetos en los que puedes interactuar
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        OnExitRange?.Invoke(this);
    }
}