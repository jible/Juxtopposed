using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The selected box's properties on the current frame.
// Text is raised on end edit, so a half typed value is never applied
public class FramePropertiesPanel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI headerLabel;
    [SerializeField] private TMP_InputField positionXInput;
    [SerializeField] private TMP_InputField positionYInput;
    [SerializeField] private Toggle activeToggle;
    [SerializeField] private TextMeshProUGUI keyStateLabel;
    [SerializeField] private Button removeKeyButton;

    public event Action<string> PositionXEdited;
    public event Action<string> PositionYEdited;
    public event Action<bool> ActiveToggled;
    public event Action RemoveKeyClicked;

    public void Awake()
    {
        positionXInput.onEndEdit.AddListener(text => PositionXEdited?.Invoke(text));
        positionYInput.onEndEdit.AddListener(text => PositionYEdited?.Invoke(text));
        activeToggle.onValueChanged.AddListener(on => ActiveToggled?.Invoke(on));
        removeKeyButton.onClick.AddListener(() => RemoveKeyClicked?.Invoke());
    }

    // Showing never raises the edit events
    public void Show(bool hasBox, int frame, string positionX, string positionY, bool active, bool hasKey, bool canRemoveKey)
    {
        headerLabel.text = $"Frame {frame}";
        positionXInput.SetTextWithoutNotify(positionX);
        positionYInput.SetTextWithoutNotify(positionY);
        activeToggle.SetIsOnWithoutNotify(active);
        keyStateLabel.text = !hasBox ? "" : hasKey ? "Key" : "Blended";

        positionXInput.interactable = hasBox;
        positionYInput.interactable = hasBox;
        activeToggle.interactable = hasBox;
        removeKeyButton.interactable = hasBox && canRemoveKey;
    }
}
