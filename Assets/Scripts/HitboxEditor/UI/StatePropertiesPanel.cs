using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// The selected state's properties: length, loop and animation.
// Text is raised on end edit, so a half typed value is never applied
public class StatePropertiesPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField lengthInput;
    [SerializeField] private Toggle loopToggle;
    [SerializeField] private TMP_InputField animationInput;

    public event Action<string> LengthEdited;
    public event Action<bool> LoopToggled;
    public event Action<string> AnimationEdited;

    public void Awake()
    {
        lengthInput.onEndEdit.AddListener(text => LengthEdited?.Invoke(text));
        loopToggle.onValueChanged.AddListener(on => LoopToggled?.Invoke(on));
        animationInput.onEndEdit.AddListener(text => AnimationEdited?.Invoke(text));
    }

    // Showing never raises the edit events. Always interactable, since editing a state with no data creates it
    public void Show(string length, bool loop, string animation)
    {
        lengthInput.SetTextWithoutNotify(length);
        loopToggle.SetIsOnWithoutNotify(loop);
        animationInput.SetTextWithoutNotify(animation);
    }
}
