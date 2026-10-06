using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

// Connects the hitbox editor's panels to HitboxEditorManager.
// Each panel raises events for its own controls, which are forwarded to the handlers below.
// The handlers call the manager, and its Changed event refreshes the panels.
// Selection and the current frame live here, since the manager only holds file data
public class HitboxEditorUI : MonoBehaviour
{
    private const int NoBox = -1;

    [SerializeField] private HitboxEditorManager manager;
    [SerializeField] private int playbackFramesPerSecond = 60;
    [Tooltip("Write the character file after every edit")]
    [SerializeField] private bool autoSave = true;

    [SerializeField] private FilePanel filePanel;
    [SerializeField] private PickerPanel pickerPanel;
    [SerializeField] private BoxPropertiesPanel boxPropertiesPanel;
    [SerializeField] private TimelinePanel timelinePanel;
    [SerializeField] private FramePropertiesPanel framePropertiesPanel;
    [SerializeField] private StatePropertiesPanel statePropertiesPanel;

    private static readonly CharacterId[] Characters = (CharacterId[])Enum.GetValues(typeof(CharacterId));
    private static readonly BoxType[] BoxTypes = (BoxType[])Enum.GetValues(typeof(BoxType));

    // A state with no data yet can still be selected, and is created on the first box added to it
    public string SelectedState { get; private set; } = HitboxEditorManager.AllStateNames[0];
    public int SelectedBoxId { get; private set; } = NoBox;
    public int CurrentFrame { get; private set; }
    public bool Playing { get; private set; }

    // Raised after every redraw, which follows every edit, frame change and selection change
    public event Action Refreshed;

    private float playbackTime;

    public void OnEnable()
    {
        manager.Changed += OnManagerChanged;

        filePanel.ReloadClicked += OnReloadClicked;
        filePanel.SaveClicked += OnSaveClicked;

        pickerPanel.CharacterSelected += OnCharacterSelected;
        pickerPanel.StateSelected += OnStateSelected;
        pickerPanel.HitGroupSelected += OnHitGroupSelected;
        pickerPanel.AddHitGroupClicked += OnAddHitGroupClicked;
        pickerPanel.RemoveHitGroupClicked += OnRemoveHitGroupClicked;
        pickerPanel.BoxSelected += OnBoxSelected;
        pickerPanel.AddBoxClicked += OnAddBoxClicked;
        pickerPanel.RemoveBoxClicked += OnRemoveBoxClicked;

        boxPropertiesPanel.NameEdited += OnBoxNameEdited;
        boxPropertiesPanel.TypeSelected += OnBoxTypeSelected;
        boxPropertiesPanel.ParentSelected += OnBoxParentSelected;
        boxPropertiesPanel.WidthEdited += OnWidthEdited;
        boxPropertiesPanel.HeightEdited += OnHeightEdited;
        boxPropertiesPanel.DamageEdited += OnDamageEdited;

        timelinePanel.FrameChanged += OnFrameChanged;
        timelinePanel.PlayClicked += OnPlayClicked;
        timelinePanel.PauseClicked += OnPauseClicked;
        timelinePanel.PreviousFrameClicked += OnPreviousFrameClicked;
        timelinePanel.NextFrameClicked += OnNextFrameClicked;

        framePropertiesPanel.PositionXEdited += OnPositionXEdited;
        framePropertiesPanel.PositionYEdited += OnPositionYEdited;
        framePropertiesPanel.ActiveToggled += OnActiveToggled;
        framePropertiesPanel.RemoveKeyClicked += OnRemoveKeyClicked;

        statePropertiesPanel.LengthEdited += OnLengthEdited;
        statePropertiesPanel.LoopToggled += OnLoopToggled;
        statePropertiesPanel.AnimationEdited += OnAnimationEdited;

        // The manager may have loaded before this was enabled
        ValidateSelection();
        Refresh();
    }

    public void OnDisable()
    {
        manager.Changed -= OnManagerChanged;

        filePanel.ReloadClicked -= OnReloadClicked;
        filePanel.SaveClicked -= OnSaveClicked;

        pickerPanel.CharacterSelected -= OnCharacterSelected;
        pickerPanel.StateSelected -= OnStateSelected;
        pickerPanel.HitGroupSelected -= OnHitGroupSelected;
        pickerPanel.AddHitGroupClicked -= OnAddHitGroupClicked;
        pickerPanel.RemoveHitGroupClicked -= OnRemoveHitGroupClicked;
        pickerPanel.BoxSelected -= OnBoxSelected;
        pickerPanel.AddBoxClicked -= OnAddBoxClicked;
        pickerPanel.RemoveBoxClicked -= OnRemoveBoxClicked;

        boxPropertiesPanel.NameEdited -= OnBoxNameEdited;
        boxPropertiesPanel.TypeSelected -= OnBoxTypeSelected;
        boxPropertiesPanel.ParentSelected -= OnBoxParentSelected;
        boxPropertiesPanel.WidthEdited -= OnWidthEdited;
        boxPropertiesPanel.HeightEdited -= OnHeightEdited;
        boxPropertiesPanel.DamageEdited -= OnDamageEdited;

        timelinePanel.FrameChanged -= OnFrameChanged;
        timelinePanel.PlayClicked -= OnPlayClicked;
        timelinePanel.PauseClicked -= OnPauseClicked;
        timelinePanel.PreviousFrameClicked -= OnPreviousFrameClicked;
        timelinePanel.NextFrameClicked -= OnNextFrameClicked;

        framePropertiesPanel.PositionXEdited -= OnPositionXEdited;
        framePropertiesPanel.PositionYEdited -= OnPositionYEdited;
        framePropertiesPanel.ActiveToggled -= OnActiveToggled;
        framePropertiesPanel.RemoveKeyClicked -= OnRemoveKeyClicked;

        statePropertiesPanel.LengthEdited -= OnLengthEdited;
        statePropertiesPanel.LoopToggled -= OnLoopToggled;
        statePropertiesPanel.AnimationEdited -= OnAnimationEdited;
    }

    // Playback always loops, whether or not the state does
    public void Update()
    {
        if (!Playing) return;
        playbackTime += Time.deltaTime;
        float frameTime = 1f / playbackFramesPerSecond;
        if (playbackTime < frameTime) return;
        while (playbackTime >= frameTime)
        {
            playbackTime -= frameTime;
            CurrentFrame = (CurrentFrame + 1) % StateLength;
        }
        Refresh();
    }

    // ---------- Manager ----------

    // Saving raises Changed again with nothing unsaved, and that second call does the refresh
    private void OnManagerChanged()
    {
        if (autoSave && manager.HasUnsavedChanges)
        {
            manager.Save();
            return;
        }
        ValidateSelection();
        Refresh();
    }

    // ---------- File ----------

    private void OnReloadClicked()
    {
        manager.LoadCharacter(manager.CharacterId);
    }

    private void OnSaveClicked()
    {
        manager.Save();
    }

    // ---------- Pickers ----------

    // Unsaved edits to the previous character are dropped, the same as a reload
    private void OnCharacterSelected(int index)
    {
        SelectedBoxId = NoBox;
        CurrentFrame = 0;
        manager.LoadCharacter(Characters[index]);
    }

    private void OnStateSelected(int index)
    {
        SelectedState = HitboxEditorManager.AllStateNames[index];
        SelectedBoxId = NoBox;
        CurrentFrame = 0;
        ValidateSelection();
        Refresh();
    }

    // Index 0 is "None". Puts the selected box in the group
    private void OnHitGroupSelected(int index)
    {
        if (!TryGetBox(out _)) return;
        int hitGroupId = index == 0 ? BoxFile.NoHitGroup : manager.GetState(SelectedState).HitGroups[index - 1].Id;
        manager.SetBoxHitGroup(SelectedState, SelectedBoxId, hitGroupId);
    }

    // Creates a group and puts the selected box in it
    private void OnAddHitGroupClicked()
    {
        if (!TryGetBox(out _)) return;
        HitGroupFile group = manager.AddHitGroup(SelectedState);
        manager.SetBoxHitGroup(SelectedState, SelectedBoxId, group.Id);
    }

    // Removes the selected box's group, which also clears it from every other box in it
    private void OnRemoveHitGroupClicked()
    {
        if (!TryGetBox(out BoxFile box) || box.HitGroup == BoxFile.NoHitGroup) return;
        manager.RemoveHitGroup(SelectedState, box.HitGroup);
    }

    private void OnBoxSelected(int index)
    {
        if (!manager.HasState(SelectedState)) return;
        SelectedBoxId = manager.GetState(SelectedState).Boxes[index].Id;
        Refresh();
    }

    private void OnAddBoxClicked()
    {
        manager.AddState(SelectedState);
        BoxFile box = manager.AddBox(SelectedState, BoxType.Hurtbox);
        SelectedBoxId = box.Id;
        Refresh();
    }

    private void OnRemoveBoxClicked()
    {
        if (!TryGetBox(out _)) return;
        int boxId = SelectedBoxId;
        SelectedBoxId = NoBox;
        manager.RemoveBox(SelectedState, boxId);
    }

    // ---------- State properties ----------

    // Editing a state with no data creates it. Bad text puts the old value back
    private void OnLengthEdited(string text)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int length))
        {
            Refresh();
            return;
        }
        manager.AddState(SelectedState);
        manager.SetStateLength(SelectedState, length);
    }

    private void OnLoopToggled(bool on)
    {
        manager.AddState(SelectedState);
        manager.SetStateLoop(SelectedState, on);
    }

    private void OnAnimationEdited(string text)
    {
        manager.AddState(SelectedState);
        manager.SetStateAnimation(SelectedState, text);
    }

    // ---------- Box properties ----------

    private void OnBoxNameEdited(string text)
    {
        if (!TryGetBox(out _)) return;
        manager.RenameBox(SelectedState, SelectedBoxId, text);
    }

    private void OnBoxTypeSelected(int index)
    {
        if (!TryGetBox(out _)) return;
        manager.SetBoxType(SelectedState, SelectedBoxId, BoxTypes[index]);
    }

    // Index 0 is the root
    private void OnBoxParentSelected(int index)
    {
        if (!TryGetBox(out BoxFile box)) return;
        List<string> parents = GetParentOptions(box);
        manager.SetParent(SelectedState, SelectedBoxId, index == 0 ? "" : parents[index]);
    }

    // Size is keyed, so this sets it on the current frame. Bad text puts the old value back
    private void OnWidthEdited(string text)
    {
        if (!TryGetBox(out _) || !TryParse(text, out DM64 width))
        {
            Refresh();
            return;
        }
        manager.SampleBox(SelectedState, SelectedBoxId, CurrentFrame, out _, out DMVector size);
        manager.SetSize(SelectedState, SelectedBoxId, CurrentFrame, new DMVector(width, size.y));
    }

    private void OnHeightEdited(string text)
    {
        if (!TryGetBox(out _) || !TryParse(text, out DM64 height))
        {
            Refresh();
            return;
        }
        manager.SampleBox(SelectedState, SelectedBoxId, CurrentFrame, out _, out DMVector size);
        manager.SetSize(SelectedState, SelectedBoxId, CurrentFrame, new DMVector(size.x, height));
    }

    // Damage belongs to the box's hit group
    private void OnDamageEdited(string text)
    {
        if (!TryGetBox(out BoxFile box) || box.HitGroup == BoxFile.NoHitGroup
            || !int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int damage))
        {
            Refresh();
            return;
        }
        HitGroupFile group = manager.GetHitGroup(SelectedState, box.HitGroup);
        manager.SetHitGroupValues(SelectedState, group.Id, group.Name, damage, group.Hitstun, group.Knockback);
    }

    // ---------- Timeline ----------

    private void OnFrameChanged(int frame)
    {
        CurrentFrame = Mathf.Clamp(frame, 0, StateLength - 1);
        Refresh();
    }

    private void OnPlayClicked()
    {
        Playing = true;
        playbackTime = 0;
        Refresh();
    }

    private void OnPauseClicked()
    {
        Playing = false;
        Refresh();
    }

    private void OnPreviousFrameClicked()
    {
        Playing = false;
        CurrentFrame = (CurrentFrame - 1 + StateLength) % StateLength;
        Refresh();
    }

    private void OnNextFrameClicked()
    {
        Playing = false;
        CurrentFrame = (CurrentFrame + 1) % StateLength;
        Refresh();
    }

    // ---------- Frame properties ----------

    private void OnPositionXEdited(string text)
    {
        if (!TryGetBox(out _) || !TryParse(text, out DM64 x))
        {
            Refresh();
            return;
        }
        manager.SampleBox(SelectedState, SelectedBoxId, CurrentFrame, out DMVector offset, out _);
        manager.SetPosition(SelectedState, SelectedBoxId, CurrentFrame, new DMVector(x, offset.y));
    }

    private void OnPositionYEdited(string text)
    {
        if (!TryGetBox(out _) || !TryParse(text, out DM64 y))
        {
            Refresh();
            return;
        }
        manager.SampleBox(SelectedState, SelectedBoxId, CurrentFrame, out DMVector offset, out _);
        manager.SetPosition(SelectedState, SelectedBoxId, CurrentFrame, new DMVector(offset.x, y));
    }

    private void OnActiveToggled(bool on)
    {
        if (!TryGetBox(out _)) return;
        bool[] active = manager.GetActiveFrames(SelectedState, SelectedBoxId);
        active[CurrentFrame] = on;
        manager.SetActiveFrames(SelectedState, SelectedBoxId, active);
    }

    private void OnRemoveKeyClicked()
    {
        if (!TryGetBox(out _)) return;
        manager.RemoveKey(SelectedState, SelectedBoxId, CurrentFrame);
    }

    // ---------- Refresh ----------

    // Redraws every panel from the manager's loaded file and the current selection
    private void Refresh()
    {
        if (manager.Loaded == null) return;

        filePanel.Show(manager.HasUnsavedChanges);

        var characterNames = new List<string>();
        foreach (var id in Characters) characterNames.Add(id.ToString());
        pickerPanel.ShowCharacters(characterNames, Array.IndexOf(Characters, manager.CharacterId));

        var stateNames = new List<string>(HitboxEditorManager.AllStateNames);
        pickerPanel.ShowStates(stateNames, stateNames.IndexOf(SelectedState));

        bool hasState = manager.HasState(SelectedState);
        bool hasBox = TryGetBox(out BoxFile box);

        var boxNames = new List<string>();
        int boxIndex = NoBox;
        if (hasState)
        {
            List<BoxFile> boxes = manager.GetState(SelectedState).Boxes;
            for (int i = 0; i < boxes.Count; i++)
            {
                boxNames.Add(boxes[i].Name);
                if (boxes[i].Id == SelectedBoxId) boxIndex = i;
            }
        }
        pickerPanel.ShowBoxes(boxNames, boxIndex);

        var hitGroupNames = new List<string> { "None" };
        int hitGroupIndex = 0;
        HitGroupFile hitGroup = null;
        if (hasState)
        {
            List<HitGroupFile> groups = manager.GetState(SelectedState).HitGroups;
            for (int i = 0; i < groups.Count; i++)
            {
                hitGroupNames.Add(groups[i].Name);
                if (hasBox && groups[i].Id == box.HitGroup)
                {
                    hitGroupIndex = i + 1;
                    hitGroup = groups[i];
                }
            }
        }
        pickerPanel.ShowHitGroups(hitGroupNames, hitGroupIndex, hasBox && box.Type == BoxType.Hitbox);

        var typeNames = new List<string>();
        foreach (var type in BoxTypes) typeNames.Add(type.ToString());

        DMVector offset = DMVector.zero;
        DMVector size = DMVector.zero;
        bool active = false;
        bool hasKey = false;
        if (hasBox)
        {
            manager.SampleBox(SelectedState, SelectedBoxId, CurrentFrame, out offset, out size);
            active = manager.GetActiveFrames(SelectedState, SelectedBoxId)[CurrentFrame];
            hasKey = manager.HasKey(SelectedState, SelectedBoxId, CurrentFrame);
        }

        List<string> parents = hasBox ? GetParentOptions(box) : new List<string> { "Root" };
        boxPropertiesPanel.Show(
            hasBox,
            hasBox ? box.Name : "",
            typeNames,
            hasBox ? Array.IndexOf(BoxTypes, box.Type) : 0,
            parents,
            hasBox && box.Bone != "" ? parents.IndexOf(box.Bone) : 0,
            hasBox ? Format(size.x) : "",
            hasBox ? Format(size.y) : "",
            hitGroup != null ? hitGroup.Damage.ToString(CultureInfo.InvariantCulture) : "",
            hitGroup != null);

        StateFile state = hasState ? manager.GetState(SelectedState) : null;
        statePropertiesPanel.Show(
            state != null ? state.Length.ToString(CultureInfo.InvariantCulture) : "",
            state != null && state.Loop,
            state != null ? state.Animation : "");

        bool[] keyFrames = null;
        if (hasBox)
        {
            keyFrames = new bool[StateLength];
            foreach (var key in box.Keys)
            {
                if (key.Frame >= 0 && key.Frame < keyFrames.Length) keyFrames[key.Frame] = true;
            }
        }
        timelinePanel.Show(CurrentFrame, StateLength, Playing, keyFrames);

        framePropertiesPanel.Show(
            hasBox,
            CurrentFrame,
            hasBox ? Format(offset.x) : "",
            hasBox ? Format(offset.y) : "",
            active,
            hasKey,
            hasKey && box.Keys.Count > 1);

        Refreshed?.Invoke();
    }

    // ---------- Helpers ----------

    // 1 for a state with no data yet
    private int StateLength => manager.Loaded != null && manager.HasState(SelectedState) ? manager.GetState(SelectedState).Length : 1;

    private bool TryGetBox(out BoxFile box)
    {
        box = null;
        if (SelectedBoxId == NoBox || manager.Loaded == null || !manager.HasState(SelectedState)) return false;
        foreach (var candidate in manager.GetState(SelectedState).Boxes)
        {
            if (candidate.Id == SelectedBoxId)
            {
                box = candidate;
                return true;
            }
        }
        return false;
    }

    // After a load or removal the selected box may be gone, so the first box is picked instead.
    // The state may also have shrunk past the current frame
    private void ValidateSelection()
    {
        if (manager.Loaded == null) return;
        if (!TryGetBox(out _))
        {
            SelectedBoxId = NoBox;
            if (manager.HasState(SelectedState) && manager.GetState(SelectedState).Boxes.Count > 0)
            {
                SelectedBoxId = manager.GetState(SelectedState).Boxes[0].Id;
            }
        }
        CurrentFrame = Mathf.Clamp(CurrentFrame, 0, StateLength - 1);
    }

    // "Root", then the bones baked for the state. The box's own bone is kept even when it has no bake
    private List<string> GetParentOptions(BoxFile box)
    {
        var parents = new List<string> { "Root" };
        parents.AddRange(manager.GetBones(SelectedState));
        if (box.Bone != "" && !parents.Contains(box.Bone)) parents.Add(box.Bone);
        return parents;
    }

    private static bool TryParse(string text, out DM64 value)
    {
        bool parsed = float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float f);
        value = new DM64(f);
        return parsed;
    }

    private static string Format(DM64 value) => value.ToFloat().ToString("0.###", CultureInfo.InvariantCulture);
}
