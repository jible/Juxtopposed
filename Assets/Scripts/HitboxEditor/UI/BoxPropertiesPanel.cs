using System;
using TMPro;
using UnityEngine;

// The selected box's properties that are the same on every frame.
// Text is raised on end edit, so a half typed value is never applied
public class BoxPropertiesPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField nameInput;
    [SerializeField] private TMP_Dropdown typeDropdown;
    [SerializeField] private TMP_Dropdown parentDropdown;
    [SerializeField] private TMP_InputField widthInput;
    [SerializeField] private TMP_InputField heightInput;
    [SerializeField] private TMP_InputField damageInput;

    public event Action<string> NameEdited;
    public event Action<int> TypeSelected;
    public event Action<int> ParentSelected;
    public event Action<string> WidthEdited;
    public event Action<string> HeightEdited;
    public event Action<string> DamageEdited;

    public void Awake()
    {
        nameInput.onEndEdit.AddListener(text => NameEdited?.Invoke(text));
        typeDropdown.onValueChanged.AddListener(index => TypeSelected?.Invoke(index));
        parentDropdown.onValueChanged.AddListener(index => ParentSelected?.Invoke(index));
        widthInput.onEndEdit.AddListener(text => WidthEdited?.Invoke(text));
        heightInput.onEndEdit.AddListener(text => HeightEdited?.Invoke(text));
        damageInput.onEndEdit.AddListener(text => DamageEdited?.Invoke(text));
    }
}
