using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Picks what is being edited: character, state, hit group and box.
// Selections are raised as dropdown indexes
public class PickerPanel : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown characterDropdown;
    [SerializeField] private TMP_Dropdown stateDropdown;

    [SerializeField] private TMP_Dropdown hitGroupDropdown;
    [SerializeField] private Button addHitGroupButton;
    [SerializeField] private Button removeHitGroupButton;

    [SerializeField] private TMP_Dropdown boxDropdown;
    [SerializeField] private Button addBoxButton;
    [SerializeField] private Button removeBoxButton;

    public event Action<int> CharacterSelected;
    public event Action<int> StateSelected;
    public event Action<int> HitGroupSelected;
    public event Action AddHitGroupClicked;
    public event Action RemoveHitGroupClicked;
    public event Action<int> BoxSelected;
    public event Action AddBoxClicked;
    public event Action RemoveBoxClicked;

    public void Awake()
    {
        characterDropdown.onValueChanged.AddListener(index => CharacterSelected?.Invoke(index));
        stateDropdown.onValueChanged.AddListener(index => StateSelected?.Invoke(index));

        hitGroupDropdown.onValueChanged.AddListener(index => HitGroupSelected?.Invoke(index));
        addHitGroupButton.onClick.AddListener(() => AddHitGroupClicked?.Invoke());
        removeHitGroupButton.onClick.AddListener(() => RemoveHitGroupClicked?.Invoke());

        boxDropdown.onValueChanged.AddListener(index => BoxSelected?.Invoke(index));
        addBoxButton.onClick.AddListener(() => AddBoxClicked?.Invoke());
        removeBoxButton.onClick.AddListener(() => RemoveBoxClicked?.Invoke());
    }

    // Showing never raises the selection events

    public void ShowCharacters(List<string> names, int selected)
    {
        Fill(characterDropdown, names, selected);
    }

    public void ShowStates(List<string> names, int selected)
    {
        Fill(stateDropdown, names, selected);
    }

    // Disabled when the selected box can not have a hit group
    public void ShowHitGroups(List<string> names, int selected, bool enabled)
    {
        Fill(hitGroupDropdown, names, selected);
        hitGroupDropdown.interactable = enabled;
        addHitGroupButton.interactable = enabled;
        removeHitGroupButton.interactable = enabled && selected > 0;
    }

    public void ShowBoxes(List<string> names, int selected)
    {
        Fill(boxDropdown, names, selected);
        boxDropdown.interactable = names.Count > 0;
        removeBoxButton.interactable = selected >= 0;
    }

    private static void Fill(TMP_Dropdown dropdown, List<string> names, int selected)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(names);
        dropdown.SetValueWithoutNotify(Mathf.Max(0, selected));
        dropdown.RefreshShownValue();
    }
}
