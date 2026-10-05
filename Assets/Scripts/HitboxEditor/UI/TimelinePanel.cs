using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Frame slider and playback buttons
public class TimelinePanel : MonoBehaviour
{
    [SerializeField] private Slider frameSlider;
    [SerializeField] private TextMeshProUGUI frameLabel;
    [SerializeField] private Button playButton;
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button previousFrameButton;
    [SerializeField] private Button nextFrameButton;

    [Tooltip("Holds one notch per frame")]
    [SerializeField] private RectTransform notchHolder;
    [Tooltip("Holds one segment per frame, showing which frames the selected box is active on")]
    [SerializeField] private RectTransform activeBand;

    // The slider uses whole numbers, so this is always a frame index
    public event Action<int> FrameChanged;
    public event Action PlayClicked;
    public event Action PauseClicked;
    public event Action PreviousFrameClicked;
    public event Action NextFrameClicked;

    public void Awake()
    {
        frameSlider.onValueChanged.AddListener(value => FrameChanged?.Invoke(Mathf.RoundToInt(value)));
        playButton.onClick.AddListener(() => PlayClicked?.Invoke());
        pauseButton.onClick.AddListener(() => PauseClicked?.Invoke());
        previousFrameButton.onClick.AddListener(() => PreviousFrameClicked?.Invoke());
        nextFrameButton.onClick.AddListener(() => NextFrameClicked?.Invoke());
    }
}
