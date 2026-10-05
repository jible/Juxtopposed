using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Reload and save buttons, and the unsaved changes warning
public class FilePanel : MonoBehaviour
{
    [SerializeField] private Button reloadButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private TextMeshProUGUI unsavedLabel;

    public event Action ReloadClicked;
    public event Action SaveClicked;

    public void Awake()
    {
        reloadButton.onClick.AddListener(() => ReloadClicked?.Invoke());
        saveButton.onClick.AddListener(() => SaveClicked?.Invoke());
    }

    public void Show(bool hasUnsavedChanges)
    {
        unsavedLabel.gameObject.SetActive(hasUnsavedChanges);
    }
}
