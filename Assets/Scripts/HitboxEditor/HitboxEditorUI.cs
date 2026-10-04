using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

// Builds the hitbox editor's controls when the scene starts.
// Left section: file buttons, character, state, hit group and hitbox pickers, then the box's static properties.
// Right section: the character display on top, then the timeline and the box's animated properties.
// Look and layout only for now: nothing is hooked up to HitboxEditorManager yet.
// Controls are exposed as properties so wiring them up later does not need to search the hierarchy.
public class HitboxEditorUI : MonoBehaviour
{
    [Tooltip("Left side, filled edge to edge with the pickers and static properties")]
    [SerializeField, FormerlySerializedAs("uiSection")]
    private RectTransform leftSection;
    [Tooltip("Right side. The timeline and animated properties fill its bottom")]
    [SerializeField, FormerlySerializedAs("displaySection")]
    private RectTransform rightSection;
    [Tooltip("Optional. Stretched to fill the right section above the timeline")]
    [SerializeField]
    private RectTransform characterDisplay;
    [Tooltip("Height of the timeline and animated properties, in canvas reference pixels")]
    [SerializeField]
    private float rightControlsHeight = 190;
    [Tooltip("Frames shown in the timeline until a state is loaded")]
    [SerializeField]
    private int previewFrameCount = 30;

    // ---------- Palette ----------

    private static readonly Color PanelColor = Hex(0x262626);
    private static readonly Color FieldColor = Hex(0x1C1C1C);
    private static readonly Color ButtonColor = Hex(0x3D3D3D);
    private static readonly Color SelectedColor = Hex(0x36485F);
    private static readonly Color MenuColor = Hex(0x333333);
    private static readonly Color DividerColor = Hex(0x3A3A3A);
    private static readonly Color TextColor = Hex(0xDADADA);
    private static readonly Color MutedTextColor = Hex(0x9A9A9A);
    private static readonly Color NotchColor = Hex(0x555555);
    private static readonly Color AccentColor = Hex(0x6AA6F8);
    private static readonly Color DangerColor = Hex(0x8C3B3B);
    private static readonly Color WarningColor = Hex(0xE0B341);
    private static readonly Color KeyColor = Hex(0xF2F2F2);

    // Public so the preview can draw boxes in the same colors
    public static readonly Color HurtboxColor = Hex(0x4CB85C);
    public static readonly Color HitboxColor = Hex(0xE5534B);
    public static readonly Color PushboxColor = Hex(0xE0B341);

    public static Color BoxTypeColor(BoxType type) => type switch
    {
        BoxType.Hurtbox => HurtboxColor,
        BoxType.Hitbox => HitboxColor,
        BoxType.Pushbox => PushboxColor,
        _ => MutedTextColor,
    };

    private const float RowHeight = 22;
    private const float FontSize = 13;
    private const float SmallFontSize = 11;
    private const float LabelWidth = 80;
    private const float Spacing = 5;
    private const float CornerRadius = 4;
    // Space kept at each end of the slider so the knob never hangs off it.
    // The track, notches, fill and knob all share it, which keeps them lined up
    private const float SliderInset = 8;
    // Every this many frames gets a taller notch
    private const int MajorNotchEvery = 5;

    // ---------- Controls ----------

    // Left: file
    public Button ReloadButton { get; private set; }
    public Button SaveButton { get; private set; }
    public TextMeshProUGUI UnsavedLabel { get; private set; }

    // Left: pickers
    public TMP_Dropdown CharacterDropdown { get; private set; }
    public TMP_Dropdown StateDropdown { get; private set; }
    public TMP_Dropdown HitGroupDropdown { get; private set; }
    public Button AddHitGroupButton { get; private set; }
    public Button RemoveHitGroupButton { get; private set; }
    public TMP_Dropdown BoxDropdown { get; private set; }
    public Button AddBoxButton { get; private set; }
    public Button RemoveBoxButton { get; private set; }

    // Left: static properties, the same on every frame
    public TMP_InputField BoxNameInput { get; private set; }
    public TMP_Dropdown BoxTypeDropdown { get; private set; }
    public TMP_Dropdown BoxParentDropdown { get; private set; }
    public TMP_InputField WidthInput { get; private set; }
    public TMP_InputField HeightInput { get; private set; }
    public TMP_InputField DamageInput { get; private set; }

    // Right: timeline
    public Slider FrameSlider { get; private set; }
    public TextMeshProUGUI FrameLabel { get; private set; }
    public Button PlayButton { get; private set; }
    public Button PauseButton { get; private set; }
    public Button PreviousFrameButton { get; private set; }
    public Button NextFrameButton { get; private set; }
    // One per frame
    public readonly List<Image> Notches = new();
    public readonly List<Image> ActiveSegments = new();

    // Right: animated properties, on the current frame
    public TextMeshProUGUI AnimatedHeaderLabel { get; private set; }
    public TMP_InputField PositionXInput { get; private set; }
    public TMP_InputField PositionYInput { get; private set; }
    public Toggle ActiveToggle { get; private set; }
    public TextMeshProUGUI KeyStateLabel { get; private set; }
    public Button RemoveKeyButton { get; private set; }

    private RectTransform notchHolder;
    private RectTransform activeBand;
    private TMP_DefaultControls.Resources tmpResources;

    // Generated at runtime, since Unity's built in UI sprites can not be loaded outside the editor
    private static Sprite roundedSprite;
    private static Sprite circleSprite;

    public void Awake()
    {
        // Unity's null check, since a scene reload can destroy them while the static fields still hold them
        if (roundedSprite == null) roundedSprite = MakeRoundedSprite(16, CornerRadius);
        if (circleSprite == null) circleSprite = MakeRoundedSprite(32, 16);
        tmpResources = new TMP_DefaultControls.Resources
        {
            standard = roundedSprite,
            background = roundedSprite,
            inputField = roundedSprite,
            checkmark = roundedSprite,
            mask = roundedSprite,
            knob = circleSprite,
        };

        SizeSections();
        if (leftSection != null) BuildLeft(leftSection);
        else Debug.LogError("Hitbox editor UI has no left section");
        if (rightSection != null) BuildRight(rightSection);
        else Debug.LogError("Hitbox editor UI has no right section");

        SetFrameCount(previewFrameCount);
        AddPlaceholderContent();
    }

    // The background's layout group splits by layout elements, so give both halves one if they have none
    private void SizeSections()
    {
        if (leftSection != null && !leftSection.TryGetComponent(out LayoutElement _))
        {
            leftSection.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        }
        if (rightSection != null && !rightSection.TryGetComponent(out LayoutElement _))
        {
            rightSection.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1.25f;
        }
    }

    // ---------- Left ----------

    private void BuildLeft(RectTransform parent)
    {
        RectTransform root = Overlay("Hitbox Editor Left", parent);
        Stretch(root);
        AddImage(root, PanelColor);
        VerticalLayout(root, 10, Spacing);

        RectTransform title = Row(root, "Title", RowHeight);
        Header(title, "HITBOX EDITOR");
        Spacer(title);
        UnsavedLabel = Label(title, "Unsaved changes", SmallFontSize, WarningColor);

        RectTransform file = Row(root, "File", RowHeight);
        ReloadButton = TextButton(file, "Reload", ButtonColor);
        SaveButton = TextButton(file, "Save", AccentColor);

        RectTransform rows = ScrollArea(root, "Rows");

        CharacterDropdown = Dropdown(FieldRow(rows, "Character:"), Enum.GetNames(typeof(CharacterId)));
        StateDropdown = Dropdown(FieldRow(rows, "State:"), HitboxEditorManager.AllStateNames);
        Divider(rows);

        RectTransform hitGroupRow = FieldRow(rows, "Hit group:");
        HitGroupDropdown = Dropdown(hitGroupRow, new[] { "None" });
        RemoveHitGroupButton = TextButton(hitGroupRow, "-", ButtonColor, RowHeight);
        AddHitGroupButton = TextButton(hitGroupRow, "+", ButtonColor, RowHeight);

        RectTransform boxRow = FieldRow(rows, "Hitbox:");
        BoxDropdown = Dropdown(boxRow, new[] { "None" });
        RemoveBoxButton = TextButton(boxRow, "-", ButtonColor, RowHeight);
        AddBoxButton = TextButton(boxRow, "+", ButtonColor, RowHeight);
        Divider(rows);

        Header(Row(rows, "Properties Header", RowHeight), "PROPERTIES");
        BoxNameInput = TextInput(FieldRow(rows, "Name:"), "Box name");
        BoxTypeDropdown = Dropdown(FieldRow(rows, "Type:"), Enum.GetNames(typeof(BoxType)));
        BoxParentDropdown = Dropdown(FieldRow(rows, "Parent:"), new[] { "Root" });

        RectTransform sizeRow = FieldRow(rows, "Size:");
        WidthInput = LabeledInput(sizeRow, "W", "0", TMP_InputField.ContentType.DecimalNumber);
        HeightInput = LabeledInput(sizeRow, "H", "0", TMP_InputField.ContentType.DecimalNumber);

        RectTransform damageRow = FieldRow(rows, "Damage:");
        DamageInput = TextInput(damageRow, "0", TMP_InputField.ContentType.IntegerNumber, 70);
    }

    // ---------- Right ----------

    private void BuildRight(RectTransform parent)
    {
        if (characterDisplay != null)
        {
            // Fill everything above the controls
            Ignore(characterDisplay);
            characterDisplay.anchorMin = Vector2.zero;
            characterDisplay.anchorMax = Vector2.one;
            characterDisplay.offsetMin = new Vector2(0, rightControlsHeight);
            characterDisplay.offsetMax = Vector2.zero;
        }

        RectTransform root = Overlay("Hitbox Editor Right", parent);
        root.anchorMin = Vector2.zero;
        root.anchorMax = new Vector2(1, 0);
        root.pivot = new Vector2(0.5f, 0);
        root.sizeDelta = new Vector2(0, rightControlsHeight);
        root.anchoredPosition = Vector2.zero;
        AddImage(root, PanelColor);
        VerticalLayout(root, 10, Spacing);

        RectTransform sliderRow = Row(root, "Timeline", 26);
        FrameSlider = NotchedSlider(sliderRow);
        FrameLabel = Label(sliderRow, "0 / 0", FontSize, TextColor, 56, TextAlignmentOptions.MidlineRight);

        RectTransform playback = Row(root, "Playback", RowHeight);
        playback.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        PlayButton = TextButton(playback, "Play", ButtonColor, 56);
        PauseButton = TextButton(playback, "Pause", ButtonColor, 56);
        PreviousFrameButton = TextButton(playback, "<-", ButtonColor, 36);
        NextFrameButton = TextButton(playback, "->", ButtonColor, 36);

        Divider(root);
        AnimatedHeaderLabel = Header(Row(root, "Animated Header", RowHeight), "FRAME 0");

        RectTransform positionRow = FieldRow(root, "Position:");
        PositionXInput = LabeledInput(positionRow, "X", "0", TMP_InputField.ContentType.DecimalNumber);
        PositionYInput = LabeledInput(positionRow, "Y", "0", TMP_InputField.ContentType.DecimalNumber);

        RectTransform activeRow = FieldRow(root, "Active:");
        ActiveToggle = CheckToggle(activeRow);
        Label(activeRow, "On this frame", SmallFontSize, MutedTextColor);
        Spacer(activeRow);
        KeyStateLabel = Label(activeRow, "Keyed", SmallFontSize, KeyColor);
        RemoveKeyButton = TextButton(activeRow, "Remove Key", DangerColor, 90);
    }

    private Slider NotchedSlider(RectTransform parent)
    {
        RectTransform root = NewRect("Frame Slider", parent);
        Layout(root, minWidth: 60, preferredWidth: 0, flexibleWidth: 1, flexibleHeight: 1);
        // Invisible, so a click anywhere along the slider grabs it
        AddImage(root, Color.clear);

        // Notches sit in the lower part, the track and knob above them
        notchHolder = NewRect("Notches", root);
        SliderBand(notchHolder, 0, 0.45f);

        // Thin strip under the notches showing which frames the selected box is on
        activeBand = NewRect("Active Band", root);
        SliderBand(activeBand, 0, 0);
        activeBand.pivot = new Vector2(0.5f, 0);
        activeBand.sizeDelta = new Vector2(-SliderInset * 2, 3);

        RectTransform track = NewRect("Track", root);
        SliderBand(track, 0.72f, 0.72f);
        track.sizeDelta = new Vector2(-SliderInset * 2, 4);
        AddImage(track, FieldColor, true).raycastTarget = false;

        RectTransform fillArea = NewRect("Fill Area", root);
        SliderBand(fillArea, 0.72f, 0.72f);
        fillArea.sizeDelta = new Vector2(-SliderInset * 2, 4);
        RectTransform fill = NewRect("Fill", fillArea);
        fill.sizeDelta = Vector2.zero;
        AddImage(fill, AccentColor, true).raycastTarget = false;

        RectTransform handleArea = NewRect("Handle Slide Area", root);
        SliderBand(handleArea, 0.72f, 0.72f);
        handleArea.sizeDelta = new Vector2(-SliderInset * 2, 12);
        RectTransform knob = NewRect("Knob", handleArea);
        knob.sizeDelta = new Vector2(12, 0);
        Image knobImage = AddImage(knob, Color.white);
        knobImage.sprite = circleSprite;

        Slider slider = root.gameObject.AddComponent<Slider>();
        slider.fillRect = fill;
        slider.handleRect = knob;
        slider.targetGraphic = knobImage;
        slider.direction = Slider.Direction.LeftToRight;
        slider.colors = Tint(TextColor);
        slider.wholeNumbers = true;
        slider.minValue = 0;
        slider.value = 0;
        return slider;
    }

    // Spans the slider's width minus SliderInset at each end, between two vertical anchors
    private static void SliderBand(RectTransform rect, float anchorMinY, float anchorMaxY)
    {
        rect.anchorMin = new Vector2(0, anchorMinY);
        rect.anchorMax = new Vector2(1, anchorMaxY);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(-SliderInset * 2, 0);
        rect.anchoredPosition = Vector2.zero;
    }

    // ---------- Timeline markers ----------

    // Rebuilds the notches and active band for a state's length, and sets the slider's range
    public void SetFrameCount(int count)
    {
        count = Mathf.Max(1, count);
        if (notchHolder != null)
        {
            ClearChildren(notchHolder);
            ClearChildren(activeBand);
            Notches.Clear();
            ActiveSegments.Clear();

            for (int frame = 0; frame < count; frame++)
            {
                // Same spacing the slider uses for whole number values, so every notch sits under a knob stop
                float t = count > 1 ? frame / (float)(count - 1) : 0;
                float halfStep = count > 1 ? 0.5f / (count - 1) : 0.5f;

                RectTransform notch = NewRect($"Notch {frame}", notchHolder);
                notch.anchorMin = notch.anchorMax = new Vector2(t, 1);
                notch.pivot = new Vector2(0.5f, 1);
                Image notchImage = AddImage(notch, NotchColor);
                notchImage.raycastTarget = false;
                Notches.Add(notchImage);

                RectTransform segment = NewRect($"Active {frame}", activeBand);
                segment.anchorMin = new Vector2(Mathf.Max(0, t - halfStep), 0);
                segment.anchorMax = new Vector2(Mathf.Min(1, t + halfStep), 1);
                segment.offsetMin = new Vector2(0.5f, 0);
                segment.offsetMax = new Vector2(-0.5f, 0);
                Image segmentImage = AddImage(segment, Color.clear);
                segmentImage.raycastTarget = false;
                ActiveSegments.Add(segmentImage);
            }

            // After the loop, since the last frame's notch is styled as major
            for (int frame = 0; frame < count; frame++)
            {
                StyleNotch(frame, false);
            }
        }

        if (FrameSlider != null)
        {
            FrameSlider.maxValue = count - 1;
            FrameLabel.text = $"{(int)FrameSlider.value} / {count - 1}";
        }
    }

    // Keyed frames get a bright notch, active frames a colored segment under the notches
    public void SetFrameMarkers(bool[] keyed, bool[] active, Color activeColor)
    {
        for (int frame = 0; frame < Notches.Count; frame++)
        {
            StyleNotch(frame, keyed != null && frame < keyed.Length && keyed[frame]);
            bool on = active != null && frame < active.Length && active[frame];
            ActiveSegments[frame].color = on ? activeColor : Color.clear;
        }
    }

    private void StyleNotch(int frame, bool keyed)
    {
        bool major = frame % MajorNotchEvery == 0 || frame == Notches.Count - 1;
        RectTransform notch = Notches[frame].rectTransform;
        if (keyed)
        {
            notch.sizeDelta = new Vector2(3, 9);
            Notches[frame].color = KeyColor;
        }
        else
        {
            notch.sizeDelta = new Vector2(1, major ? 8 : 4);
            Notches[frame].color = major ? MutedTextColor : NotchColor;
        }
    }

    private void AddPlaceholderContent()
    {
        if (BoxDropdown != null)
        {
            SetOptions(BoxDropdown, new[] { "Body (Hurtbox)", "Head (Hurtbox)", "Sweet spot (Hitbox)", "Sour spot (Hitbox)" });
            SetOptions(HitGroupDropdown, new[] { "None", "Hit 0" });
        }

        var keyed = new bool[Notches.Count];
        var active = new bool[Notches.Count];
        foreach (int frame in new[] { 0, 4, 12 })
        {
            if (frame < keyed.Length) keyed[frame] = true;
        }
        for (int frame = 4; frame <= 12 && frame < active.Length; frame++)
        {
            active[frame] = true;
        }
        SetFrameMarkers(keyed, active, HitboxColor);
    }

    public static void SetOptions(TMP_Dropdown dropdown, IEnumerable<string> options)
    {
        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
    }

    // ---------- Composite controls ----------

    // Vertical list that scrolls when its rows do not fit
    private RectTransform ScrollArea(RectTransform parent, string name)
    {
        RectTransform scrollArea = NewRect(name, parent);
        Layout(scrollArea, minHeight: 0, preferredHeight: 0, flexibleWidth: 1, flexibleHeight: 1);
        // Clear still catches the scroll wheel
        AddImage(scrollArea, Color.clear);
        ScrollRect scroll = scrollArea.gameObject.AddComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 20;

        RectTransform viewport = NewRect("Viewport", scrollArea);
        Stretch(viewport);
        viewport.gameObject.AddComponent<RectMask2D>();

        RectTransform content = NewRect("Content", viewport);
        content.anchorMin = new Vector2(0, 1);
        content.anchorMax = new Vector2(1, 1);
        content.pivot = new Vector2(0.5f, 1);
        content.sizeDelta = Vector2.zero;
        VerticalLayout(content, 0, Spacing);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        return content;
    }

    // A "Label:" column followed by whatever controls get added to the row
    private RectTransform FieldRow(RectTransform parent, string label)
    {
        RectTransform row = Row(parent, label + " Row", RowHeight);
        Label(row, label, FontSize, TextColor, LabelWidth);
        return row;
    }

    // Small axis label then a field, like "X: [ 0 ]"
    private TMP_InputField LabeledInput(RectTransform parent, string axis, string placeholder, TMP_InputField.ContentType contentType)
    {
        Label(parent, axis + ":", SmallFontSize, MutedTextColor, 16);
        return TextInput(parent, placeholder, contentType);
    }

    private void Divider(RectTransform parent)
    {
        RectTransform divider = NewRect("Divider", parent);
        Layout(divider, flexibleWidth: 1, minHeight: 1, preferredHeight: 1);
        AddImage(divider, DividerColor).raycastTarget = false;
    }

    // ---------- Basic controls ----------

    private TextMeshProUGUI Label(RectTransform parent, string text, float size, Color color, float width = -1,
        TextAlignmentOptions alignment = TextAlignmentOptions.MidlineLeft, bool flexible = false)
    {
        RectTransform rect = NewRect(text.Length > 0 ? text : "Label", parent);
        TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = size;
        label.color = color;
        label.alignment = alignment;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;
        if (width >= 0) Layout(rect, minWidth: width, preferredWidth: width);
        if (flexible) Layout(rect, minWidth: 0, preferredWidth: 0, flexibleWidth: 1);
        return label;
    }

    private TextMeshProUGUI Header(RectTransform parent, string text)
    {
        TextMeshProUGUI header = Label(parent, text, SmallFontSize, MutedTextColor);
        header.fontStyle = FontStyles.Bold;
        header.characterSpacing = 6;
        return header;
    }

    // Width 0 shares the row with the other flexible controls
    private Button TextButton(RectTransform parent, string text, Color color, float width = 0)
    {
        RectTransform rect = NewRect(text + " Button", parent);
        if (width > 0) Layout(rect, minWidth: width, preferredWidth: width, preferredHeight: RowHeight);
        else Layout(rect, minWidth: 0, preferredWidth: 0, flexibleWidth: 1, preferredHeight: RowHeight);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = AddImage(rect, Color.white, true);
        button.colors = Tint(color);

        RectTransform labelRect = NewRect("Label", rect);
        Stretch(labelRect);
        TextMeshProUGUI label = labelRect.gameObject.AddComponent<TextMeshProUGUI>();
        label.text = text;
        label.fontSize = FontSize;
        label.color = TextColor;
        label.alignment = TextAlignmentOptions.Center;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.raycastTarget = false;
        return button;
    }

    // Width 0 fills the rest of the row
    private TMP_InputField TextInput(RectTransform parent, string placeholder,
        TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard, float width = 0)
    {
        GameObject go = TMP_DefaultControls.CreateInputField(tmpResources);
        RectTransform rect = Adopt(go, parent);
        if (width > 0) Layout(rect, minWidth: width, preferredWidth: width, preferredHeight: RowHeight);
        else Layout(rect, minWidth: 24, preferredWidth: 0, flexibleWidth: 1, preferredHeight: RowHeight);

        TMP_InputField input = go.GetComponent<TMP_InputField>();
        go.GetComponent<Image>().color = Color.white;
        input.colors = Tint(FieldColor);
        input.contentType = contentType;
        input.pointSize = FontSize;
        input.customCaretColor = true;
        input.caretColor = TextColor;
        input.selectionColor = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.4f);

        // The default text area insets are sized for a taller field
        var textArea = (RectTransform)go.transform.Find("Text Area");
        textArea.offsetMin = new Vector2(7, 1);
        textArea.offsetMax = new Vector2(-7, -1);

        var text = (TextMeshProUGUI)input.textComponent;
        text.color = TextColor;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        var placeholderText = (TextMeshProUGUI)input.placeholder;
        placeholderText.text = placeholder;
        placeholderText.fontStyle = FontStyles.Normal;
        placeholderText.color = new Color(MutedTextColor.r, MutedTextColor.g, MutedTextColor.b, 0.5f);
        placeholderText.alignment = TextAlignmentOptions.MidlineLeft;
        return input;
    }

    // Width 0 fills the rest of the row
    private TMP_Dropdown Dropdown(RectTransform parent, IEnumerable<string> options, float width = 0)
    {
        GameObject go = TMP_DefaultControls.CreateDropdown(tmpResources);
        RectTransform rect = Adopt(go, parent);
        if (width > 0) Layout(rect, minWidth: width, preferredWidth: width, preferredHeight: RowHeight);
        else Layout(rect, minWidth: 40, preferredWidth: 0, flexibleWidth: 1, preferredHeight: RowHeight);

        TMP_Dropdown dropdown = go.GetComponent<TMP_Dropdown>();
        go.GetComponent<Image>().color = Color.white;
        dropdown.colors = Tint(ButtonColor);

        var caption = (TextMeshProUGUI)dropdown.captionText;
        caption.fontSize = FontSize;
        caption.color = TextColor;
        caption.alignment = TextAlignmentOptions.MidlineLeft;
        caption.textWrappingMode = TextWrappingModes.NoWrap;
        caption.overflowMode = TextOverflowModes.Ellipsis;
        caption.rectTransform.offsetMin = new Vector2(8, 0);
        caption.rectTransform.offsetMax = new Vector2(-20, 0);

        // No arrow sprite is given, so draw a chevron with text instead
        Transform arrow = go.transform.Find("Arrow");
        arrow.GetComponent<Image>().color = Color.clear;
        RectTransform chevronRect = NewRect("Chevron", (RectTransform)arrow);
        Stretch(chevronRect);
        TextMeshProUGUI chevron = chevronRect.gameObject.AddComponent<TextMeshProUGUI>();
        chevron.text = "v";
        chevron.fontSize = SmallFontSize;
        chevron.color = MutedTextColor;
        chevron.alignment = TextAlignmentOptions.Center;
        chevron.raycastTarget = false;

        // The popup list
        RectTransform template = dropdown.template;
        template.GetComponent<Image>().color = MenuColor;
        Transform scrollbar = template.Find("Scrollbar");
        scrollbar.GetComponent<Image>().color = MenuColor;
        scrollbar.Find("Sliding Area/Handle").GetComponent<Image>().color = ButtonColor;

        var itemLabel = (TextMeshProUGUI)dropdown.itemText;
        itemLabel.fontSize = FontSize;
        itemLabel.color = TextColor;
        itemLabel.alignment = TextAlignmentOptions.MidlineLeft;

        Transform item = itemLabel.transform.parent;
        ((RectTransform)item).sizeDelta = new Vector2(0, RowHeight);
        Image itemBackground = item.Find("Item Background").GetComponent<Image>();
        itemBackground.color = Color.white;
        itemBackground.sprite = null;
        Toggle itemToggle = item.GetComponent<Toggle>();
        ColorBlock itemColors = Tint(MenuColor);
        itemColors.highlightedColor = SelectedColor;
        itemColors.selectedColor = SelectedColor;
        itemToggle.colors = itemColors;

        // The checkmark becomes an accent stripe on the selected item
        var checkmark = (RectTransform)item.Find("Item Checkmark");
        checkmark.anchorMin = Vector2.zero;
        checkmark.anchorMax = new Vector2(0, 1);
        checkmark.pivot = new Vector2(0, 0.5f);
        checkmark.anchoredPosition = new Vector2(3, 0);
        checkmark.sizeDelta = new Vector2(3, -8);
        checkmark.GetComponent<Image>().color = AccentColor;

        SetOptions(dropdown, options);
        return dropdown;
    }

    private Toggle CheckToggle(RectTransform parent)
    {
        RectTransform rect = NewRect("Toggle", parent);
        Layout(rect, minWidth: 16, preferredWidth: 16, preferredHeight: 16);
        Image background = AddImage(rect, Color.white, true);

        RectTransform check = NewRect("Check", rect);
        Stretch(check, 4);
        Image checkImage = AddImage(check, AccentColor, true);
        checkImage.raycastTarget = false;

        Toggle toggle = rect.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkImage;
        toggle.colors = Tint(FieldColor);
        toggle.isOn = true;
        return toggle;
    }

    private RectTransform Row(RectTransform parent, string name, float height)
    {
        RectTransform row = NewRect(name, parent);
        Layout(row, flexibleWidth: 1, minHeight: height, preferredHeight: height);
        HorizontalLayout(row, 0, Spacing);
        return row;
    }

    private void Spacer(RectTransform parent, float width = -1)
    {
        RectTransform spacer = NewRect("Spacer", parent);
        if (width >= 0) Layout(spacer, minWidth: width, preferredWidth: width);
        else Layout(spacer, minWidth: 0, preferredWidth: 0, flexibleWidth: 1);
    }

    // ---------- Helpers ----------

    // A child the section's own layout group leaves alone
    private static RectTransform Overlay(string name, RectTransform parent)
    {
        RectTransform rect = NewRect(name, parent);
        Ignore(rect);
        return rect;
    }

    private static void Ignore(RectTransform rect)
    {
        if (!rect.TryGetComponent(out LayoutElement element)) element = rect.gameObject.AddComponent<LayoutElement>();
        element.ignoreLayout = true;
    }

    private static RectTransform NewRect(string name, RectTransform parent)
    {
        var go = new GameObject(name, typeof(RectTransform));
        return Adopt(go, parent);
    }

    private static RectTransform Adopt(GameObject go, RectTransform parent)
    {
        go.layer = parent.gameObject.layer;
        var rect = (RectTransform)go.transform;
        rect.SetParent(parent, false);
        return rect;
    }

    private static void Stretch(RectTransform rect, float inset = 0)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(inset, inset);
        rect.offsetMax = new Vector2(-inset, -inset);
    }

    private static Image AddImage(RectTransform rect, Color color, bool rounded = false)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        if (rounded)
        {
            image.sprite = roundedSprite;
            image.type = Image.Type.Sliced;
        }
        return image;
    }

    private static VerticalLayoutGroup VerticalLayout(RectTransform rect, int padding, float spacing)
    {
        var layout = rect.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        return layout;
    }

    private static HorizontalLayoutGroup HorizontalLayout(RectTransform rect, int padding, float spacing)
    {
        var layout = rect.gameObject.AddComponent<HorizontalLayoutGroup>();
        layout.padding = new RectOffset(padding, padding, padding, padding);
        layout.spacing = spacing;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return layout;
    }

    // Only overrides the values passed, -1 leaves a value to the element's own layout
    private static LayoutElement Layout(RectTransform rect, float minWidth = -1, float preferredWidth = -1, float flexibleWidth = -1,
        float minHeight = -1, float preferredHeight = -1, float flexibleHeight = -1)
    {
        if (!rect.TryGetComponent(out LayoutElement element)) element = rect.gameObject.AddComponent<LayoutElement>();
        if (minWidth >= 0) element.minWidth = minWidth;
        if (preferredWidth >= 0) element.preferredWidth = preferredWidth;
        if (flexibleWidth >= 0) element.flexibleWidth = flexibleWidth;
        if (minHeight >= 0) element.minHeight = minHeight;
        if (preferredHeight >= 0) element.preferredHeight = preferredHeight;
        if (flexibleHeight >= 0) element.flexibleHeight = flexibleHeight;
        return element;
    }

    // Images are white so the color block alone decides the color
    private static ColorBlock Tint(Color color)
    {
        ColorBlock colors = ColorBlock.defaultColorBlock;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.1f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.15f);
        colors.selectedColor = color;
        colors.disabledColor = Color.Lerp(color, PanelColor, 0.6f);
        colors.colorMultiplier = 1;
        colors.fadeDuration = 0.06f;
        return colors;
    }

    private static void ClearChildren(RectTransform rect)
    {
        for (int i = rect.childCount - 1; i >= 0; i--)
        {
            // Detach first so the layout ignores it before the deferred destroy runs
            Transform child = rect.GetChild(i);
            child.SetParent(null, false);
            Destroy(child.gameObject);
        }
    }

    // White rounded square with soft edges, sliced so the corners keep their size at any scale.
    // A radius of half the size makes a circle
    private static Sprite MakeRoundedSprite(int size, float radius)
    {
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = $"Rounded {size} {radius}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
        };
        var pixels = new Color32[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // Distance from the nearest point of the inner square the corners round off
                float px = x + 0.5f;
                float py = y + 0.5f;
                float cx = Mathf.Clamp(px, radius, size - radius);
                float cy = Mathf.Clamp(py, radius, size - radius);
                float distance = Vector2.Distance(new Vector2(px, py), new Vector2(cx, cy));
                float alpha = Mathf.Clamp01(radius - distance + 0.5f);
                pixels[y * size + x] = new Color32(255, 255, 255, (byte)(alpha * 255));
            }
        }
        texture.SetPixels32(pixels);
        texture.Apply();

        float border = Mathf.Min(radius + 1, size / 2f);
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0,
            SpriteMeshType.FullRect, new Vector4(border, border, border, border));
    }

    private static Color Hex(uint rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
}
