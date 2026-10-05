using UnityEngine;

// Connects the hitbox editor's panels to HitboxEditorManager.
// Each panel raises events for its own controls, which are forwarded to the handlers below,
// and the manager's Changed event refreshes the panels. The handlers are stubs for now.
public class HitboxEditorUI : MonoBehaviour
{
    [SerializeField] private HitboxEditorManager manager;

    [SerializeField] private FilePanel filePanel;
    [SerializeField] private PickerPanel pickerPanel;
    [SerializeField] private BoxPropertiesPanel boxPropertiesPanel;
    [SerializeField] private TimelinePanel timelinePanel;
    [SerializeField] private FramePropertiesPanel framePropertiesPanel;

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
    }

    // ---------- Manager ----------

    // After every edit, load and save. Refresh the panels from the loaded file
    private void OnManagerChanged()
    {
    }

    // ---------- File ----------

    private void OnReloadClicked()
    {
    }

    private void OnSaveClicked()
    {
    }

    // ---------- Pickers ----------

    private void OnCharacterSelected(int index)
    {
    }

    private void OnStateSelected(int index)
    {
    }

    private void OnHitGroupSelected(int index)
    {
    }

    private void OnAddHitGroupClicked()
    {
    }

    private void OnRemoveHitGroupClicked()
    {
    }

    private void OnBoxSelected(int index)
    {
    }

    private void OnAddBoxClicked()
    {
    }

    private void OnRemoveBoxClicked()
    {
    }

    // ---------- Box properties ----------

    private void OnBoxNameEdited(string text)
    {
    }

    private void OnBoxTypeSelected(int index)
    {
    }

    private void OnBoxParentSelected(int index)
    {
    }

    private void OnWidthEdited(string text)
    {
    }

    private void OnHeightEdited(string text)
    {
    }

    private void OnDamageEdited(string text)
    {
    }

    // ---------- Timeline ----------

    private void OnFrameChanged(int frame)
    {
    }

    private void OnPlayClicked()
    {
    }

    private void OnPauseClicked()
    {
    }

    private void OnPreviousFrameClicked()
    {
    }

    private void OnNextFrameClicked()
    {
    }

    // ---------- Frame properties ----------

    private void OnPositionXEdited(string text)
    {
    }

    private void OnPositionYEdited(string text)
    {
    }

    private void OnActiveToggled(bool on)
    {
    }

    private void OnRemoveKeyClicked()
    {
    }
}
