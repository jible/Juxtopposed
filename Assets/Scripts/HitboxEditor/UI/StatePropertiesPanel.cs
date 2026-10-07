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
    [Tooltip("Shown when the length doesn't split evenly over the animation's sprites")]
    [SerializeField] private TextMeshProUGUI warningLabel;
    [SerializeField] private Color warningColor = new Color(1f, 0.85f, 0.1f);

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
    // warning is empty when there is nothing to warn about
    public void Show(string length, bool loop, string animation, string warning)
    {
        lengthInput.SetTextWithoutNotify(length);
        loopToggle.SetIsOnWithoutNotify(loop);
        animationInput.SetTextWithoutNotify(animation);
        warningLabel.text = warning;
        warningLabel.color = warningColor;
        warningLabel.gameObject.SetActive(!string.IsNullOrEmpty(warning));
    }
}
