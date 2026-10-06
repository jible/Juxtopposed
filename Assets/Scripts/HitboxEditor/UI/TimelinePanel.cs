using System;
using System.Collections.Generic;
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

    // Notch sizes: every frame, every fifth frame, and frames where the selected box has a key
    private static readonly Vector2 FrameNotchSize = new Vector2(1, 4);
    private static readonly Vector2 FifthNotchSize = new Vector2(1, 8);
    private static readonly Vector2 KeyNotchSize = new Vector2(3, 9);
    private const int NotchStep = 5;

    // The holder's first child is cloned for the rest, so the scene sets their look
    private readonly List<RectTransform> notches = new();

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

        CollectChildren(notchHolder, notches);
    }

    // Showing never raises FrameChanged. keys has one entry per frame, or is null with no box selected
    public void Show(int frame, int length, bool playing, bool[] keys)
    {
        frameSlider.wholeNumbers = true;
        frameSlider.minValue = 0;
        frameSlider.maxValue = Mathf.Max(0, length - 1);
        frameSlider.SetValueWithoutNotify(frame);
        frameLabel.text = $"{frame} / {length - 1}";
        playButton.interactable = !playing;
        pauseButton.interactable = playing;

        ShowNotches(length, keys);
    }

    // One notch per slider position, so notch i sits under the handle on frame i
    private void ShowNotches(int length, bool[] keys)
    {
        EnsureCount(notches, length);
        for (int i = 0; i < notches.Count; i++)
        {
            RectTransform notch = notches[i];
            bool shown = i < length;
            notch.gameObject.SetActive(shown);
            if (!shown) continue;

            float x = length > 1 ? (float)i / (length - 1) : 0;
            notch.anchorMin = new Vector2(x, notch.anchorMin.y);
            notch.anchorMax = new Vector2(x, notch.anchorMax.y);
            notch.anchoredPosition = Vector2.zero;
            notch.sizeDelta = keys != null && keys[i] ? KeyNotchSize : i % NotchStep == 0 ? FifthNotchSize : FrameNotchSize;
        }
    }

    private static void CollectChildren(RectTransform holder, List<RectTransform> list)
    {
        foreach (Transform child in holder)
        {
            list.Add((RectTransform)child);
        }
    }

    // Clones the first child until there are enough. Extras are kept and hidden
    private static void EnsureCount(List<RectTransform> list, int count)
    {
        if (list.Count == 0) return;
        while (list.Count < count)
        {
            RectTransform clone = Instantiate(list[0], list[0].parent);
            clone.name = $"{list[0].name} {list.Count}";
            list.Add(clone);
        }
    }
}
