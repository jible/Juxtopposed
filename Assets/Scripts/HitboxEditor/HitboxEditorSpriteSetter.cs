using UnityEngine;

// Shows the character's sprite in the hitbox editor on the selected state and timeline frame.
// Draws through the same view the game uses, so boxes are placed against exactly what plays.
// Sits on the object with the SpriteRenderer, which stands in for the character root
public class HitboxEditorSpriteSetter : MonoBehaviour
{
    [SerializeField] private HitboxEditorManager manager;
    [SerializeField] private HitboxEditorUI ui;

    private SpriteCharacterView view;
    // The character the view's sheets are loaded for, so switching characters reloads them
    private CharacterId? configuredFor;

    public void Awake()
    {
        view = gameObject.AddComponent<SpriteCharacterView>();
    }

    // The UI refreshes after every edit, frame change and selection change
    public void OnEnable()
    {
        ui.Refreshed += Show;
    }

    public void OnDisable()
    {
        ui.Refreshed -= Show;
    }

    // Sprites the state's animation has, so the timeline can step one sprite at a time. 0 when there is no sheet
    public int SpriteCount(string stateName)
    {
        if (manager.Loaded == null) return 0;
        EnsureConfigured();
        StateDefinition state = manager.Definition.GetState(stateName);
        return view.SpriteCount(state != null ? state.AnimationName : stateName);
    }

    private void Show()
    {
        if (manager.Loaded == null) return;
        EnsureConfigured();

        // Matches Character.LateUpdate, with the timeline frame standing in for ticks in state
        StateDefinition state = manager.Definition.GetState(ui.SelectedState);
        if (state != null)
        {
            view.Show(state.AnimationName, ui.CurrentFrame, state.Length, Character.Direction.Right);
        }
        else
        {
            view.Show(ui.SelectedState, ui.CurrentFrame, 0, Character.Direction.Right);
        }
    }

    private void EnsureConfigured()
    {
        if (configuredFor == manager.CharacterId) return;
        view.Configure(CharacterFiles.ResourceFolder(manager.CharacterId), Roster.Get(manager.CharacterId).Visuals.DefaultAnimation);
        configuredFor = manager.CharacterId;
    }
}
