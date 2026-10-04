using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Builds the hitbox editor's controls inside the UI section when the scene starts.
// Look and layout only for now: nothing is hooked up to HitboxEditorManager yet.
// Controls are exposed as properties so wiring them up later does not need to search the hierarchy.
public class HitboxEditorUI : MonoBehaviour
{
    [Tooltip("Filled edge to edge. Falls back to the canvas when empty")]
    [SerializeField]
    private RectTransform uiSection;
    [SerializeField]
    private Canvas canvas;
    [Tooltip("Frames shown in the timeline until a state is loaded")]
    [SerializeField]
    private int previewFrameCount = 30;

    // ---------- Palette ----------

    private static readonly Color PanelColor = Hex(0x1B1C21);
    private static readonly Color CardColor = Hex(0x24262C);
    private static readonly Color FieldColor = Hex(0x31343C);
    private static readonly Color ButtonColor = Hex(0x3A3E48);
    private static readonly Color SelectedColor = Hex(0x34405A);
    private static readonly Color MenuColor = Hex(0x2B2E35);
    private static readonly Color TextColor = Hex(0xE4E6EB);
    private static readonly Color MutedTextColor = Hex(0x8E95A3);
    private static readonly Color AccentColor = Hex(0x4C8DFF);
    private static readonly Color DangerColor = Hex(0xB84A47);
    private static readonly Color WarningColor = Hex(0xE0B341);
    private static readonly Color KeyColor = Hex(0xF2F2F2);

    // Public so the preview can draw boxes in the same colors as the list
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
    private const float LabelWidth = 64;
    private const float Spacing = 4;

    // ---------- Controls ----------

    // Toolbar
    public TMP_Dropdown CharacterDropdown { get; private set; }
    public TMP_Dropdown StateDropdown { get; private set; }
    public TextMeshProUGUI UnsavedLabel { get; private set; }
    public Button ReloadButton { get; private set; }
    public Button SaveButton { get; private set; }

    // Timeline
    public Button PreviousFrameButton { get; private set; }
    public Button PlayButton { get; private set; }
    public Button NextFrameButton { get; private set; }
    public TextMeshProUGUI FrameLabel { get; private set; }
    public Slider FrameSlider { get; private set; }
    // One per frame
    public readonly List<Toggle> ActiveFrameToggles = new();
    public readonly List<Image> KeyMarkers = new();

    // Box list
    public Button AddHurtboxButton { get; private set; }
    public Button AddHitboxButton { get; private set; }
    public Button AddPushboxButton { get; private set; }
    public Button RemoveBoxButton { get; private set; }
    public RectTransform BoxListContent { get; private set; }

    // Selected box
    public TMP_InputField BoxNameInput { get; private set; }
    public TMP_Dropdown BoxTypeDropdown { get; private set; }
    public TMP_Dropdown BoxParentDropdown { get; private set; }
    public Toggle KeepPositionToggle { get; private set; }
    public TMP_Dropdown BoxHitGroupDropdown { get; private set; }

    // Key on the current frame
    public TextMeshProUGUI KeyHeaderLabel { get; private set; }
    public TMP_InputField PositionXInput { get; private set; }
    public TMP_InputField PositionYInput { get; private set; }
    public TMP_InputField SizeXInput { get; private set; }
    public TMP_InputField SizeYInput { get; private set; }
    public TMP_Dropdown InterpolationDropdown { get; private set; }
    public Button AddKeyButton { get; private set; }
    public Button RemoveKeyButton { get; private set; }

    // State and its hit groups
    public TMP_InputField AnimationInput { get; private set; }
    public TMP_InputField LengthInput { get; private set; }
    public Toggle LoopToggle { get; private set; }
    public TMP_Dropdown HitGroupDropdown { get; private set; }
    public Button AddHitGroupButton { get; private set; }
    public Button RemoveHitGroupButton { get; private set; }
    public TMP_InputField DamageInput { get; private set; }
    public TMP_InputField HitstunInput { get; private set; }
    public TMP_InputField KnockbackXInput { get; private set; }
    public TMP_InputField KnockbackYInput { get; private set; }

    private RectTransform activeStrip;
    private RectTransform keyStrip;
    // Sprites are left empty so every control draws as a flat colored rect
    private readonly TMP_DefaultControls.Resources tmpResources = new();

    public void Awake()
    {
        Build();
    }

    private void Build()
    {
        RectTransform parent = uiSection != null ? uiSection : (RectTransform)canvas.transform;
        RectTransform root = NewRect("Hitbox Editor UI", parent);
        Stretch(root);
        // The section's own layout group should not resize this
        root.gameObject.AddComponent<LayoutElement>().ignoreLayout = true;
        AddImage(root, PanelColor);
        VerticalLayout(root, 8, 6);

        BuildToolbar(root);
        BuildTimeline(root);
        BuildBody(root);

        SetFrameCount(previewFrameCount);
        AddPlaceholderContent();
    }

    private void BuildToolbar(RectTransform root)
    {
        RectTransform bar = Row(root, "Toolbar", RowHeight + 4);
        Label(bar, "Character", FontSize, MutedTextColor);
        CharacterDropdown = Dropdown(bar, Enum.GetNames(typeof(CharacterId)), 130);
        Spacer(bar, 8);
        Label(bar, "State", FontSize, MutedTextColor);
        StateDropdown = Dropdown(bar, HitboxEditorManager.AllStateNames, 130);
        Spacer(bar);
        UnsavedLabel = Label(bar, "Unsaved changes", SmallFontSize, WarningColor);
        ReloadButton = TextButton(bar, "Reload", ButtonColor, 70);
        SaveButton = TextButton(bar, "Save", AccentColor, 70);
    }

    private void BuildTimeline(RectTransform root)
    {
        RectTransform card = Card(root, "Timeline", 0, 0);

        RectTransform controls = Row(card, "Controls", RowHeight);
        PreviousFrameButton = TextButton(controls, "<", ButtonColor, 28);
        PlayButton = TextButton(controls, "Play", ButtonColor, 52);
        NextFrameButton = TextButton(controls, ">", ButtonColor, 28);
        FrameLabel = Label(controls, "Frame 0 / 0", FontSize, TextColor, 100, TextAlignmentOptions.Center);
        FrameSlider = FrameScrubber(controls);

        RectTransform activeRow = Row(card, "Active Row", 16);
        Label(activeRow, "Active", SmallFontSize, MutedTextColor, LabelWidth);
        activeStrip = Strip(activeRow, "Active Frames");

        RectTransform keyRow = Row(card, "Key Row", 8);
        Label(keyRow, "Keys", SmallFontSize, MutedTextColor, LabelWidth);
        keyStrip = Strip(keyRow, "Key Markers");
    }

    private void BuildBody(RectTransform root)
    {
        RectTransform body = NewRect("Body", root);
        Layout(body, flexibleWidth: 1, flexibleHeight: 1);
        HorizontalLayoutGroup layout = HorizontalLayout(body, 0, 6);
        layout.childForceExpandHeight = true;

        BuildBoxList(body);
        BuildBoxPanel(body);
        BuildKeyPanel(body);
        BuildStatePanel(body);
    }

    private void BuildBoxList(RectTransform body)
    {
        RectTransform card = Card(body, "Boxes", 1, 1);
        RectTransform header = Row(card, "Header", RowHeight);
        Header(header, "BOXES");
        Spacer(header);
        RemoveBoxButton = TextButton(header, "Remove", DangerColor, 64);

        RectTransform add = Row(card, "Add", RowHeight);
        AddHurtboxButton = TextButton(add, "+ Hurt", ButtonColor, 0, HurtboxColor);
        AddHitboxButton = TextButton(add, "+ Hit", ButtonColor, 0, HitboxColor);
        AddPushboxButton = TextButton(add, "+ Push", ButtonColor, 0, PushboxColor);

        BoxListContent = ScrollList(card, "Box List");
    }

    private void BuildBoxPanel(RectTransform body)
    {
        RectTransform card = Card(body, "Box", 1.1f, 1);
        Header(Row(card, "Header", RowHeight), "BOX");

        BoxNameInput = TextInput(FieldRow(card, "Name"), "Box name");
        BoxTypeDropdown = Dropdown(FieldRow(card, "Type"), Enum.GetNames(typeof(BoxType)));

        RectTransform parentRow = FieldRow(card, "Parent");
        BoxParentDropdown = Dropdown(parentRow, new[] { "Root" });

        RectTransform keepRow = FieldRow(card, "");
        KeepPositionToggle = CheckToggle(keepRow);
        KeepPositionToggle.isOn = true;
        Label(keepRow, "Keep position on reparent", SmallFontSize, MutedTextColor);

        BoxHitGroupDropdown = Dropdown(FieldRow(card, "Hit group"), new[] { "None" });
    }

    private void BuildKeyPanel(RectTransform body)
    {
        RectTransform card = Card(body, "Key", 1.1f, 1);
        KeyHeaderLabel = Header(Row(card, "Header", RowHeight), "KEY  ·  FRAME 0");

        VectorRow(card, "Position", out TMP_InputField positionX, out TMP_InputField positionY);
        PositionXInput = positionX;
        PositionYInput = positionY;
        VectorRow(card, "Size", out TMP_InputField sizeX, out TMP_InputField sizeY);
        SizeXInput = sizeX;
        SizeYInput = sizeY;
        InterpolationDropdown = Dropdown(FieldRow(card, "Blend"), Enum.GetNames(typeof(KeyInterpolation)));

        RectTransform buttons = Row(card, "Buttons", RowHeight);
        AddKeyButton = TextButton(buttons, "Add Key", ButtonColor);
        RemoveKeyButton = TextButton(buttons, "Remove Key", DangerColor);
    }

    private void BuildStatePanel(RectTransform body)
    {
        RectTransform card = Card(body, "State", 1.2f, 1);
        Header(Row(card, "Header", RowHeight), "STATE");

        AnimationInput = TextInput(FieldRow(card, "Animation"), "Animation path");

        RectTransform lengthRow = FieldRow(card, "Length");
        LengthInput = TextInput(lengthRow, "Frames", TMP_InputField.ContentType.IntegerNumber);
        Label(lengthRow, "Loop", SmallFontSize, MutedTextColor);
        LoopToggle = CheckToggle(lengthRow);

        RectTransform hitHeader = Row(card, "Hit Group Header", RowHeight);
        Header(hitHeader, "HIT GROUPS");
        Spacer(hitHeader);
        AddHitGroupButton = TextButton(hitHeader, "+", ButtonColor, 24);
        RemoveHitGroupButton = TextButton(hitHeader, "-", DangerColor, 24);

        HitGroupDropdown = Dropdown(FieldRow(card, "Group"), new[] { "None" });

        RectTransform damageRow = FieldRow(card, "Dmg / Stun");
        DamageInput = TextInput(damageRow, "Damage", TMP_InputField.ContentType.IntegerNumber);
        HitstunInput = TextInput(damageRow, "Ticks", TMP_InputField.ContentType.IntegerNumber);

        VectorRow(card, "Knockback", out TMP_InputField knockbackX, out TMP_InputField knockbackY);
        KnockbackXInput = knockbackX;
        KnockbackYInput = knockbackY;
    }

    // ---------- Rebuildable parts ----------

    // Rebuilds the per frame strips and the scrubber range
    public void SetFrameCount(int count)
    {
        count = Mathf.Max(1, count);
        ClearChildren(activeStrip);
        ClearChildren(keyStrip);
        ActiveFrameToggles.Clear();
        KeyMarkers.Clear();

        for (int frame = 0; frame < count; frame++)
        {
            ActiveFrameToggles.Add(FrameCell(activeStrip, frame));

            RectTransform marker = NewRect($"Key {frame}", keyStrip);
            Layout(marker, minWidth: 2, preferredWidth: 0, flexibleWidth: 1, preferredHeight: 8);
            KeyMarkers.Add(AddImage(marker, Color.clear));
        }

        FrameSlider.maxValue = count - 1;
        FrameLabel.text = $"Frame {(int)FrameSlider.value} / {count - 1}";
    }

    public void ClearBoxList() => ClearChildren(BoxListContent);

    public Button AddBoxListItem(string boxName, BoxType type, bool selected)
    {
        RectTransform item = NewRect(boxName, BoxListContent);
        Layout(item, preferredHeight: RowHeight);
        Button button = item.gameObject.AddComponent<Button>();
        button.targetGraphic = AddImage(item, Color.white);
        button.colors = Tint(selected ? SelectedColor : CardColor);
        HorizontalLayout(item, 0, 6).padding = new RectOffset(0, 6, 0, 0);

        RectTransform tag = NewRect("Type Tag", item);
        Layout(tag, minWidth: 4, preferredWidth: 4, flexibleHeight: 1);
        AddImage(tag, BoxTypeColor(type));

        Label(item, boxName, FontSize, TextColor, flexible: true);
        Label(item, type.ToString(), SmallFontSize, MutedTextColor);
        return button;
    }

    private void AddPlaceholderContent()
    {
        ClearBoxList();
        AddBoxListItem("Body", BoxType.Hurtbox, true);
        AddBoxListItem("Head", BoxType.Hurtbox, false);
        AddBoxListItem("Jab", BoxType.Hitbox, false);
        AddBoxListItem("Push", BoxType.Pushbox, false);

        for (int frame = 4; frame <= 12 && frame < ActiveFrameToggles.Count; frame++)
        {
            ActiveFrameToggles[frame].isOn = true;
        }
        foreach (int frame in new[] { 0, 4, 12 })
        {
            if (frame < KeyMarkers.Count) KeyMarkers[frame].color = KeyColor;
        }
    }

    // ---------- Composite controls ----------

    private RectTransform Card(RectTransform parent, string name, float flexibleWidth, float flexibleHeight)
    {
        RectTransform card = NewRect(name, parent);
        AddImage(card, CardColor);
        VerticalLayout(card, 6, Spacing);
        // Zero preferred width so columns split purely by their flexible weights
        Layout(card, minWidth: 0, preferredWidth: flexibleWidth > 0 ? 0 : -1, flexibleWidth: flexibleWidth, flexibleHeight: flexibleHeight);
        return card;
    }

    // A label column followed by whatever controls get added to the row
    private RectTransform FieldRow(RectTransform parent, string label)
    {
        RectTransform row = Row(parent, label + " Row", RowHeight);
        Label(row, label, SmallFontSize, MutedTextColor, LabelWidth);
        return row;
    }

    private void VectorRow(RectTransform parent, string label, out TMP_InputField x, out TMP_InputField y)
    {
        RectTransform row = FieldRow(parent, label);
        Label(row, "X", SmallFontSize, MutedTextColor, 10);
        x = TextInput(row, "0", TMP_InputField.ContentType.DecimalNumber);
        Label(row, "Y", SmallFontSize, MutedTextColor, 10);
        y = TextInput(row, "0", TMP_InputField.ContentType.DecimalNumber);
    }

    // Horizontal row the per frame cells are laid out in
    private RectTransform Strip(RectTransform parent, string name)
    {
        RectTransform strip = NewRect(name, parent);
        Layout(strip, minWidth: 0, preferredWidth: 0, flexibleWidth: 1, flexibleHeight: 1);
        HorizontalLayout(strip, 0, 1).childForceExpandHeight = true;
        return strip;
    }

    private Toggle FrameCell(RectTransform parent, int frame)
    {
        RectTransform cell = NewRect($"Frame {frame}", parent);
        Layout(cell, minWidth: 2, preferredWidth: 0, flexibleWidth: 1);
        Image background = AddImage(cell, Color.white);

        RectTransform fill = NewRect("Fill", cell);
        Stretch(fill, 2);
        Image fillImage = AddImage(fill, AccentColor);

        Toggle toggle = cell.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = fillImage;
        toggle.colors = Tint(FieldColor);
        toggle.isOn = false;
        return toggle;
    }

    private RectTransform ScrollList(RectTransform parent, string name)
    {
        RectTransform scrollArea = NewRect(name, parent);
        Layout(scrollArea, flexibleWidth: 1, flexibleHeight: 1);
        AddImage(scrollArea, PanelColor);
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
        VerticalLayout(content, 2, 2);
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        scroll.viewport = viewport;
        scroll.content = content;
        return content;
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

    // Width 0 shares the row with the other flexible controls. An accent color draws a stripe on the left
    private Button TextButton(RectTransform parent, string text, Color color, float width = 0, Color? accent = null)
    {
        RectTransform rect = NewRect(text + " Button", parent);
        if (width > 0) Layout(rect, minWidth: width, preferredWidth: width, preferredHeight: RowHeight);
        else Layout(rect, minWidth: 0, preferredWidth: 0, flexibleWidth: 1, preferredHeight: RowHeight);

        Button button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = AddImage(rect, Color.white);
        button.colors = Tint(color);

        if (accent.HasValue)
        {
            RectTransform stripe = NewRect("Accent", rect);
            stripe.anchorMin = Vector2.zero;
            stripe.anchorMax = new Vector2(0, 1);
            stripe.pivot = new Vector2(0, 0.5f);
            stripe.sizeDelta = new Vector2(3, 0);
            AddImage(stripe, accent.Value).raycastTarget = false;
        }

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

    private TMP_InputField TextInput(RectTransform parent, string placeholder,
        TMP_InputField.ContentType contentType = TMP_InputField.ContentType.Standard)
    {
        GameObject go = TMP_DefaultControls.CreateInputField(tmpResources);
        RectTransform rect = Adopt(go, parent);
        Layout(rect, minWidth: 24, preferredWidth: 0, flexibleWidth: 1, preferredHeight: RowHeight);

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
        textArea.offsetMin = new Vector2(6, 1);
        textArea.offsetMax = new Vector2(-6, -1);

        var text = (TextMeshProUGUI)input.textComponent;
        text.color = TextColor;
        text.alignment = TextAlignmentOptions.MidlineLeft;

        var placeholderText = (TextMeshProUGUI)input.placeholder;
        placeholderText.text = placeholder;
        placeholderText.color = new Color(MutedTextColor.r, MutedTextColor.g, MutedTextColor.b, 0.6f);
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
        dropdown.colors = Tint(FieldColor);

        var caption = (TextMeshProUGUI)dropdown.captionText;
        caption.fontSize = FontSize;
        caption.color = TextColor;
        caption.alignment = TextAlignmentOptions.MidlineLeft;
        caption.textWrappingMode = TextWrappingModes.NoWrap;
        caption.overflowMode = TextOverflowModes.Ellipsis;
        caption.rectTransform.offsetMin = new Vector2(8, 0);
        caption.rectTransform.offsetMax = new Vector2(-20, 0);

        // The default arrow is a sprite, which is a plain square without one, so draw a chevron with text instead
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
        item.Find("Item Background").GetComponent<Image>().color = Color.white;
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
        checkmark.anchoredPosition = new Vector2(2, 0);
        checkmark.sizeDelta = new Vector2(3, -6);
        checkmark.GetComponent<Image>().color = AccentColor;

        dropdown.ClearOptions();
        dropdown.AddOptions(new List<string>(options));
        return dropdown;
    }

    private Toggle CheckToggle(RectTransform parent)
    {
        RectTransform rect = NewRect("Toggle", parent);
        Layout(rect, minWidth: 16, preferredWidth: 16, preferredHeight: 16);
        Image background = AddImage(rect, Color.white);

        RectTransform check = NewRect("Check", rect);
        Stretch(check, 4);
        Image checkImage = AddImage(check, AccentColor);

        Toggle toggle = rect.gameObject.AddComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkImage;
        toggle.colors = Tint(FieldColor);
        toggle.isOn = false;
        return toggle;
    }

    private Slider FrameScrubber(RectTransform parent)
    {
        GameObject go = DefaultControls.CreateSlider(new DefaultControls.Resources());
        RectTransform rect = Adopt(go, parent);
        Layout(rect, minWidth: 40, preferredWidth: 0, flexibleWidth: 1, preferredHeight: 16);

        Slider slider = go.GetComponent<Slider>();
        go.transform.Find("Background").GetComponent<Image>().color = FieldColor;
        go.transform.Find("Fill Area/Fill").GetComponent<Image>().color = new Color(AccentColor.r, AccentColor.g, AccentColor.b, 0.5f);

        // A thin playhead instead of the default square knob
        var handle = (RectTransform)go.transform.Find("Handle Slide Area/Handle");
        handle.sizeDelta = new Vector2(6, 0);
        handle.GetComponent<Image>().color = Color.white;
        slider.colors = Tint(TextColor);

        slider.wholeNumbers = true;
        slider.minValue = 0;
        slider.value = 0;
        return slider;
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

    private static Image AddImage(RectTransform rect, Color color)
    {
        Image image = rect.gameObject.AddComponent<Image>();
        image.color = color;
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

    private static Color Hex(uint rgb) => new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
}
